using Hazel;

public class RivalCellIdNetworkMessage : NetworkMessage<RivalCellIdNetworkMessage>
{
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.RivalCellId;
    public int CellId { get; set; }
    public int ConnectionId { get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(CellId);
        messageWriter.Write(ConnectionId);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader)
    {
        CellId = messageReader.ReadInt32();
        ConnectionId = messageReader.ReadInt32();
    }
}
