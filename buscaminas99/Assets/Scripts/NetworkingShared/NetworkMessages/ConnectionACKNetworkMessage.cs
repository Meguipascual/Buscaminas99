using Hazel;

public class ConnectionACKNetworkMessage : NetworkMessage<ConnectionACKNetworkMessage>
{
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.ConnectionACK;
    public int PlayerId {  get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter)
    {
        messageWriter.Write(PlayerId);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader)
    {
        PlayerId = messageReader.ReadInt32();
    }
}
