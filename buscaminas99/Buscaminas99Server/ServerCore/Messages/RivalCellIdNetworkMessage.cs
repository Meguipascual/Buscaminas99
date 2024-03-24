using Hazel;

namespace ServerCore.Messages; 

public class RivalCellIdNetworkMessage : NetworkMessage
{
    public int CellId { get; set; }
    public int ConnectionId { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.RivalCellId);
        messageWriter.Write(CellId);
        messageWriter.Write(ConnectionId);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static RivalCellIdNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new RivalCellIdNetworkMessage();
        message.CellId = messageReader.ReadInt32();
        message.ConnectionId = messageReader.ReadInt32();
        return message;
    }
}