namespace ServerCore; 

public class ServerController : IDisposable {

    private const int MinPlayersToStartGame = 2;

    private bool _isResetting;

    private readonly ConnectionsManager _connectionsManager;
    private readonly PlayersManager _playersManager;
    private readonly MessageHandler _messageHandler;
    private readonly ServerState _serverState;

    public ServerController(
        ConnectionsManager connectionsManager, 
        PlayersManager playersManager, 
        MessageHandler messageHandler,
        ServerState serverState) {
        _connectionsManager = connectionsManager;
        _playersManager = playersManager;
        _messageHandler = messageHandler;
        _serverState = serverState;

        _playersManager.OnPlayerAdded += StartGameIfEnoughPlayers;
        _messageHandler.OnResetServerRequestReceived += ResetServerAfterWaitTime;
    }

    private Task StartGameIfEnoughPlayers(int playerId) {
        return _playersManager.PlayersCount >= MinPlayersToStartGame ? StartGame() : Task.CompletedTask;
    }

    private Task StartGame() {
        _serverState.StartGame();
        var gameStartedMessage = new GameStartedNetworkMessage {
            StartTimestamp = _serverState.StartTimestamp,
            GameDurationSeconds = ServerState.GameDurationSeconds,
        };
        _connectionsManager.BroadcastMessage(gameStartedMessage);
        return FinishGameOnceTimeIsOver();
    }

    private async Task ResetServerAfterWaitTime() {
        if (_isResetting) {
            return;
        }

        _isResetting = true;
        _connectionsManager.BroadcastEmptyMessage(NetworkMessageTypes.ResetGameWarning);
        await Task.Delay(5000);
        ResetServer();
    }

    private void ResetServer() {
        _connectionsManager.BroadcastEmptyMessage(NetworkMessageTypes.ResetGame);
        _connectionsManager.Reset();
        _playersManager.Reset();
        _isResetting = false;
    }
    
    private Task FinishGameOnceTimeIsOver() {
        const int MsPerSecond = 1000;
        Task.Delay(ServerState.GameDurationSeconds * MsPerSecond).ContinueWith(t => FinishGame());
        return Task.CompletedTask;
    }

    private Task FinishGame() {
        Console.WriteLine("Game ended");
        _playersManager.FinishGame();
        return Task.CompletedTask;
    }

    public void Dispose() {
        _playersManager.OnPlayerAdded -= StartGameIfEnoughPlayers;
        _messageHandler.OnResetServerRequestReceived -= ResetServerAfterWaitTime;
    }
}