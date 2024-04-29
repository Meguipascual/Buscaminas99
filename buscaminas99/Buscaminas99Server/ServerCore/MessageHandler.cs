using Hazel;

namespace ServerCore; 

public class MessageHandler : IDisposable {

    private readonly ConnectionsManager _connectionsManager;
    private readonly ServerState _serverState;

    public delegate Task SeedNetworkMessageReceivedDelegate(int connectionId, SeedNetworkMessage seedNetworkMessage);
    public event SeedNetworkMessageReceivedDelegate OnSeedNetworkMessageReceived = null!;
    public delegate Task ResetServerRequestReceivedDelegate();
    public event ResetServerRequestReceivedDelegate OnResetServerRequestReceived = null!;
    public delegate Task UndoPlayNetworkMessageReceivedDelegate(int targetPlayerId, UndoPlayNetworkMessage undoPlayNetworkMessage);
    public event UndoPlayNetworkMessageReceivedDelegate OnUndoPlayNetworkMessageReceived = null!;
    public delegate Task CellIdNetworkMessageReceivedDelegate(int connectionId, CellIdNetworkMessage message);
    public event CellIdNetworkMessageReceivedDelegate OnCellIdMessageReceived = null!;
    public delegate Task BoardFinishedNetworkMessageReceivedDelegate(int connectionId);
    public event BoardFinishedNetworkMessageReceivedDelegate OnBoardFinishedNetworkMessageReceived = null!;
    
    public MessageHandler(ConnectionsManager connectionsManager, ServerState serverState) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnMessageReceived += ProcessMessage;

        _serverState = serverState;
    }

    private async Task ProcessMessage(int connectionId, MessageReader messageReader) {
        INetworkMessage? networkMessage = null;
        switch ((NetworkMessageTypes)messageReader.Tag)
        {
            case NetworkMessageTypes.Seed:
                var seedMessage = SeedNetworkMessage.FromMessageReader(messageReader);
                await OnSeedNetworkMessageReceived.Invoke(connectionId, seedMessage);
                break;
            case NetworkMessageTypes.CellId:
                if (_serverState.IsGameActive) {
                    var cellIdMessage = CellIdNetworkMessage.FromMessageReader(messageReader);
                    networkMessage = new RivalCellIdNetworkMessage { ConnectionId = connectionId, CellId = cellIdMessage.CellId };
                	OnCellIdMessageReceived?.Invoke(connectionId, cellIdMessage);
                    Console.WriteLine($"Rival Cell Id received: {cellIdMessage.CellId}");
                }
                break;
            case NetworkMessageTypes.ResetServer:
                Console.WriteLine($"Reset server request received");
                OnResetServerRequestReceived?.Invoke();
                break;
            case NetworkMessageTypes.UndoPlay:
                var undoPlayMessage = UndoPlayNetworkMessage.FromMessageReader(messageReader);
                OnUndoPlayNetworkMessageReceived?.Invoke(connectionId, undoPlayMessage);
                Console.WriteLine($"Undo server request received");
                break;
            case NetworkMessageTypes.BoardFinished:
                OnBoardFinishedNetworkMessageReceived?.Invoke(connectionId);
                break;
            default: 
                Console.WriteLine($"Invalid message tag received: {messageReader.Tag}");
                break;
        }

        if (networkMessage != null) {
            _connectionsManager.SendMessageToAllConnectionsExceptOne(connectionId, networkMessage);
        }
    }

    public void Dispose() {
        _connectionsManager.OnMessageReceived -= ProcessMessage;
    }
}