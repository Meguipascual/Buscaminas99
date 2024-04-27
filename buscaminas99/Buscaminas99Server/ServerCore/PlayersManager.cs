using System.Collections.Generic;
using static Hazel.Udp.FewerThreads.ThreadLimitedUdpConnectionListener;

namespace ServerCore; 

public sealed class PlayersManager : IDisposable {

    private readonly ConnectionsManager _connectionsManager;
    private readonly MessageHandler _messageHandler;
    
    private readonly Dictionary<int, Player> _playersByConnectionId = new();
    private readonly Dictionary<int, int> _seedsByConnectionId = new();

    public delegate Task PlayerAddedDelegate(int playerId);
    public event PlayerAddedDelegate OnPlayerAdded; 

    public int PlayersCount => _playersByConnectionId.Count;

    public PlayersManager(ConnectionsManager connectionsManager, MessageHandler messageHandler) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnConnectionCreated += HandleNewPlayerConnection;

        _messageHandler = messageHandler;
        _messageHandler.OnSeedNetworkMessageReceived += SetPlayerSeed;
        _messageHandler.OnUndoPlayNetworkMessageReceived += DoThings;


    }

    public void Reset() {
        _seedsByConnectionId.Clear();
        _playersByConnectionId.Clear();
    }

    private Task HandleNewPlayerConnection(int connectionId)
    {
        BroadcastExistingPlayerSeeds(connectionId);
        CreatePlayer(connectionId);
        return Task.CompletedTask;
    }

    private Task BroadcastExistingPlayerSeeds(int connectionId) {
        foreach(var keyValuePair in _seedsByConnectionId)
        {
            var message = new RivalSeedNetworkMessage { ConnectionId = keyValuePair.Key, Seed = keyValuePair.Value };
            _connectionsManager.SendMessageToConnection(connectionId, message);
        }

        return Task.CompletedTask;
    }

    private Task SetPlayerSeed(int connectionId, SeedNetworkMessage seedNetworkMessage) {
        _seedsByConnectionId[connectionId] = seedNetworkMessage.Seed;
        return Task.CompletedTask;
    }

    private async Task CreatePlayer(int connectionId)
    {
        var player = new Player();
        player.PlayerId = connectionId;
        _playersByConnectionId.Add(connectionId, player);
        _connectionsManager.SendMessageToConnection(connectionId, new ConnectionACKNetworkMessage { PlayerId = connectionId });
        _connectionsManager.SendMessageToAllConnectionsExceptOne(connectionId, new NewPlayerConnectedNetworkMessage { PlayerId = connectionId });
        await OnPlayerAdded.Invoke(connectionId);
    }

    public Player GetPlayer(int playerId)
    {
        return _playersByConnectionId[playerId];
    }

    public void SavePlayerPlay(int connectionId, CellIdNetworkMessage message)
    {
        _playersByConnectionId[connectionId].PushPlay(message);
    }

    public void DoThings(int connectionId, UndoPlayNetworkMessage undoPlayNetworkMessage)
    {
        var player = GetPlayer(undoPlayNetworkMessage.TargetPlayerId);
        var play = player.PopPlay();
        var message = new UndoMessageCommandNetworkMessage { TargetPlayerId = player.PlayerId, DiscoverCellIds = play.DiscoverCellIds.ToList() };
        _connectionsManager.BroadcastMessage(message);
    }

    public void Dispose() {
        _connectionsManager.OnConnectionCreated -= HandleNewPlayerConnection;
        _messageHandler.OnSeedNetworkMessageReceived -= SetPlayerSeed;
        _messageHandler.OnUndoPlayNetworkMessageReceived -= DoThings;
    }
}