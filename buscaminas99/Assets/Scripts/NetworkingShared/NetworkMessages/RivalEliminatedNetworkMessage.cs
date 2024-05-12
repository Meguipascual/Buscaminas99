using Hazel;

public class RivalEliminatedNetworkMessage : NetworkMessage<RivalEliminatedNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.RivalEliminated;
    public int PlayerId { get; set; }
    
    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(PlayerId);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        PlayerId = messageReader.ReadInt32();
    }
}