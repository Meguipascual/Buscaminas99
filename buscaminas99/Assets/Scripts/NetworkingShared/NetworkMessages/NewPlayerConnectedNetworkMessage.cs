using Hazel;

public class NewPlayerConnectedNetworkMessage : NetworkMessage<NewPlayerConnectedNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.NewPlayerConnected;
    public int PlayerId { get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(PlayerId);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        PlayerId = messageReader.ReadInt32();
    }
}
