namespace ServerCore; 

public sealed class PlayersManager : IDisposable {

    private readonly ConnectionsManager _connectionsManager;
    private readonly MessageHandler _messageHandler;
    
    private readonly Dictionary<int, int> _seedsByConnectionId = new();

    public PlayersManager(ConnectionsManager connectionsManager, MessageHandler messageHandler) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnConnectionCreated += BroadcastExistingPlayerSeeds;

        _messageHandler = messageHandler;
        _messageHandler.OnSeedNetworkMessageReceived += SetPlayerSeed;
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

    public void Dispose() {
        _connectionsManager.OnConnectionCreated -= BroadcastExistingPlayerSeeds;
        _messageHandler.OnSeedNetworkMessageReceived -= SetPlayerSeed;
    }
}