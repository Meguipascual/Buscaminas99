using Hazel;

public class UndoPlayNetworkMessage : NetworkMessage
{
    public int TargetPlayerId {  get; set; }
    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.UndoPlay); 
        messageWriter.Write(TargetPlayerId);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static UndoPlayNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new UndoPlayNetworkMessage();
        message.TargetPlayerId = messageReader.ReadInt32();
        return message;
    }
}
