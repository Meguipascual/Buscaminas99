using static Hazel.Udp.FewerThreads.ThreadLimitedUdpConnectionListener;

namespace ServerCore; 

public sealed class PlayersManager : IDisposable {

    private readonly ConnectionsManager _connectionsManager;
    private readonly MessageHandler _messageHandler;
    
    private readonly Dictionary<int, Player> _playersByConnectionId = new();
    private readonly Dictionary<int, int> _seedsByConnectionId = new();

    public PlayersManager(ConnectionsManager connectionsManager, MessageHandler messageHandler) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnConnectionCreated += HandleNewPlayerConnection;

        _messageHandler = messageHandler;
        _messageHandler.OnSeedNetworkMessageReceived += SetPlayerSeed;
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

    private void CreatePlayer(int connectionId)
    {
        var player = new Player();
        _playersByConnectionId.Add(connectionId, player);
    }

    public void SavePlayerPlay(int connectionId, CellIdNetworkMessage message)
    {
        _playersByConnectionId[connectionId].SavePlay(message);
    }

    public void Dispose() {
        _connectionsManager.OnConnectionCreated -= HandleNewPlayerConnection;
        _messageHandler.OnSeedNetworkMessageReceived -= SetPlayerSeed;
    }
}