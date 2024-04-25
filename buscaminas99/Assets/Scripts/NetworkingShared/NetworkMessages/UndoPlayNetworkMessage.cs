using Hazel;

public class UndoPlayNetworkMessage : NetworkMessage
{
    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.UndoPlay);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static UndoPlayNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new UndoPlayNetworkMessage();
        return message;
    }
}
