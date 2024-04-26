namespace ServerCore; 

public class ServerController : IDisposable {

    private const int MinPlayersToStartGame = 2;

    private readonly ConnectionsManager _connectionsManager;
    private readonly PlayersManager _playersManager;
    private readonly MessageHandler _messageHandler;

    private bool _isResetting;
    private long _startTimestamp;

    public ServerController(ConnectionsManager connectionsManager, PlayersManager playersManager, MessageHandler messageHandler) {
        _connectionsManager = connectionsManager;
        _playersManager = playersManager;
        _messageHandler = messageHandler;

        _playersManager.OnPlayerAdded += StartGameIfEnoughPlayers;
        _messageHandler.OnResetServerRequestReceived += ResetServerAfterWaitTime;
    }

    private Task StartGameIfEnoughPlayers(int playerId) {
        return _playersManager.PlayersCount >= MinPlayersToStartGame ? StartGame() : Task.CompletedTask;
    }

    private Task StartGame() {
        _startTimestamp = (DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        var gameStartedMessage = new GameStartedNetworkMessage() {
            StartTimestamp = _startTimestamp
        };
        _connectionsManager.BroadcastMessage(gameStartedMessage);
        return Task.CompletedTask;
    }

    private async Task ResetServerAfterWaitTime() {
        if (_isResetting) {
            return;
        }
        
        _connectionsManager.BroadcastEmptyMessage(NetworkMessageTypes.ResetGameWarning);
        await Task.Delay(5000);
        ResetServer();
    }

    private void ResetServer() {
        _connectionsManager.BroadcastEmptyMessage(NetworkMessageTypes.ResetGame);
        _connectionsManager.Reset();
        _playersManager.Reset();
    }

    public void Dispose() {
        _playersManager.OnPlayerAdded -= StartGameIfEnoughPlayers;
        _messageHandler.OnResetServerRequestReceived -= ResetServerAfterWaitTime;
    }
}