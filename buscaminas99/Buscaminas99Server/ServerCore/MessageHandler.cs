using Hazel;

namespace ServerCore; 

public class MessageHandler : IDisposable {

    private readonly ConnectionsManager _connectionsManager;

    public delegate Task SeedNetworkMessageReceivedDelegate(int connectionId, SeedNetworkMessage seedNetworkMessage);
    public event SeedNetworkMessageReceivedDelegate OnSeedNetworkMessageReceived = null!;

    public MessageHandler(ConnectionsManager connectionsManager) {
        _connectionsManager = connectionsManager;
        _connectionsManager.OnMessageReceived += ProcessMessage;
    }

    private async Task ProcessMessage(int connectionId, MessageReader messageReader) {
        NetworkMessage? networkMessage = null;
        switch ((NetworkMessageTypes)messageReader.Tag)
        {
            case NetworkMessageTypes.Seed:
                var seedMessage = SeedNetworkMessage.FromMessageReader(messageReader);
                await OnSeedNetworkMessageReceived.Invoke(connectionId, seedMessage);
                networkMessage = new RivalSeedNetworkMessage { ConnectionId = connectionId, Seed = seedMessage.Seed };
                Console.WriteLine($"Seed received for player {connectionId}: {seedMessage.Seed}");
                break;
            case NetworkMessageTypes.CellId:
                var cellIdMessage = CellIdNetworkMessage.FromMessageReader(messageReader);
                networkMessage = new RivalCellIdNetworkMessage { ConnectionId = connectionId, CellId = cellIdMessage.CellId };
                Console.WriteLine($"Rival Cell Id received: {cellIdMessage.CellId}");
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