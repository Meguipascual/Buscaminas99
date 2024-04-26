using Hazel;

public class UndoPlayNetworkMessage : NetworkMessage<UndoPlayNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.UndoPlay;

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
    }
}
