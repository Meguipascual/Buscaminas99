using System.Collections.Generic;
using System.Diagnostics;
using static Hazel.Udp.FewerThreads.ThreadLimitedUdpConnectionListener;

namespace ServerCore; 

public sealed class PlayersManager : IDisposable {

    private readonly ConnectionsManager _connectionsManager;
    private readonly MessageHandler _messageHandler;
    private readonly ServerState _serverState;
    
    private readonly Dictionary<int, Player> _playersByConnectionId = new();

    public delegate Task PlayerAddedDelegate(int playerId);
    public event PlayerAddedDelegate OnPlayerAdded; 

    public int PlayersCount => _playersByConnectionId.Count;

    public PlayersManager(
        ConnectionsManager connectionsManager, 
        MessageHandler messageHandler,
        ServerState serverState) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnConnectionCreated += HandleNewPlayerConnection;

        _messageHandler = messageHandler;
        _messageHandler.OnSeedNetworkMessageReceived += SetPlayerSeed;
        _messageHandler.OnUndoPlayNetworkMessageReceived += UndoPlay;
        _messageHandler.OnCellIdMessageReceived += SavePlayerPlay;
        _messageHandler.OnBoardFinishedNetworkMessageReceived += TrackBoardFinished;

        _serverState = serverState;
    }

    public void Reset() {
        _playersByConnectionId.Clear();
    }

    private async Task HandleNewPlayerConnection(int connectionId)
    {
        await BroadcastExistingPlayerSeeds(connectionId);
        await CreatePlayer(connectionId);
    }

    private Task BroadcastExistingPlayerSeeds(int connectionId) {
        foreach(var keyValuePair in _playersByConnectionId)
        {
            var message = new RivalSeedNetworkMessage { ConnectionId = keyValuePair.Key, Seed = keyValuePair.Value.Seed!.Value };
            _connectionsManager.SendMessageToConnection(connectionId, message);
        }

        return Task.CompletedTask;
    }

    private Task SetPlayerSeed(int connectionId, SeedNetworkMessage seedNetworkMessage) {
        Console.WriteLine($"Seed received for player {connectionId}: {seedNetworkMessage.Seed}");
        var player = _playersByConnectionId[connectionId];
        if (player.Seed != null && !player.HasFinishedBoard) {
            return Task.CompletedTask;
        }
        
        player.RestartSeed(seedNetworkMessage.Seed);
        var networkMessage = new RivalSeedNetworkMessage { ConnectionId = connectionId, Seed = seedNetworkMessage.Seed };
        _connectionsManager.SendMessageToAllConnectionsExceptOne(connectionId, networkMessage);
        return Task.CompletedTask;
    }

    private async Task CreatePlayer(int connectionId)
    {
        var player = new Player();
        player.PlayerId = connectionId;
        _playersByConnectionId.Add(connectionId, player);
        
        _connectionsManager.SendMessageToConnection(connectionId, new ConnectionACKNetworkMessage { PlayerId = connectionId });
        
        // Let existing players know about new player
        _connectionsManager.SendMessageToAllConnectionsExceptOne(connectionId, new NewPlayerConnectedNetworkMessage { PlayerId = connectionId });
        
        // Let new player know about existing players
        foreach (var rivalPlayer in _playersByConnectionId.Values.Where(p => p.PlayerId != connectionId)) {
            _connectionsManager.SendMessageToConnection(connectionId, new NewPlayerConnectedNetworkMessage{ PlayerId = rivalPlayer.PlayerId });
        }
        await OnPlayerAdded.Invoke(connectionId);
    }

    public Player GetPlayer(int playerId)
    {
        return _playersByConnectionId[playerId];
    }

    public void FinishGame() {
        var scores = new List<GameEndedPlayerScoreDto>();
        foreach (var kvp in _playersByConnectionId) {
            var scoreDto = new GameEndedPlayerScoreDto {
                PlayerId = kvp.Value.PlayerId,
                Score = kvp.Value.Score,
            };
            scores.Add(scoreDto);
        }

        var gameEndedMessage = new GameEndedNetworkMessage {
            Scores = scores.ToList()
        };
        
        _connectionsManager.BroadcastMessage(gameEndedMessage);
    }

    private Task SavePlayerPlay(int connectionId, CellIdNetworkMessage message)
    {
        Console.WriteLine($"Play received for player:{connectionId} message:{message}");
        _playersByConnectionId[connectionId].PushPlay(message);
        return Task.CompletedTask;
    }

    private Task UndoPlay(int connectionId, UndoPlayNetworkMessage undoPlayNetworkMessage)
    {
        //Change method name plis
        var player = GetPlayer(undoPlayNetworkMessage.TargetPlayerId);
        var play = player.PopPlay();
        if (play == null) 
        {
            Console.WriteLine($"There are no plays in this player (Id:{undoPlayNetworkMessage.TargetPlayerId}) stack");
        }
        else
        {
            var message = new UndoMessageCommandNetworkMessage { TargetPlayerId = player.PlayerId, DiscoverCellIds = play.DiscoverCellIds.ToList() };
            _connectionsManager.BroadcastMessage(message);
        }

        return Task.CompletedTask;
    }

    private Task TrackBoardFinished(int connectionId) {
        var points = _serverState.EndTimestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _playersByConnectionId[connectionId].TrackBoardFinished((int)points);

        var scoreUpdatedNetworkMessage = new ScoreUpdatedNetworkMessage {
            PlayerId = connectionId,
            Score = _playersByConnectionId[connectionId].Score
        };
        _connectionsManager.BroadcastMessage(scoreUpdatedNetworkMessage);
        
        return Task.CompletedTask;
    }

    public void Dispose() {
        _connectionsManager.OnConnectionCreated -= HandleNewPlayerConnection;
        _messageHandler.OnSeedNetworkMessageReceived -= SetPlayerSeed;
        _messageHandler.OnUndoPlayNetworkMessageReceived -= UndoPlay;
        _messageHandler.OnCellIdMessageReceived -= SavePlayerPlay;
        _messageHandler.OnBoardFinishedNetworkMessageReceived -= TrackBoardFinished;
    }
}