using Hazel;

public class UndoPlayNetworkMessage : NetworkMessage<UndoPlayNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.UndoPlay;
    public int TargetPlayerId {  get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(TargetPlayerId);
    }

	protected override void FromMessageReaderImpl(MessageReader messageReader){
        TargetPlayerId = messageReader.ReadInt32();
	}
}
