namespace ServerCore; 

public class ServerController {

    private readonly ConnectionsManager _connectionsManager;
    private readonly PlayersManager _playersManager;
    private readonly MessageHandler _messageHandler;

    private bool _isResetting;

    public ServerController(ConnectionsManager connectionsManager, PlayersManager playersManager, MessageHandler messageHandler) {
        _connectionsManager = connectionsManager;
        _playersManager = playersManager;
        _messageHandler = messageHandler;

        _messageHandler.OnResetServerRequestReceived += ResetServerAfterWaitTime;
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
}