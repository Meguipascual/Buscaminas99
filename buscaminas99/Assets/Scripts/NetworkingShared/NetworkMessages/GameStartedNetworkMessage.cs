using Hazel;

public class GameStartedNetworkMessage : NetworkMessage {
    public int StartTimestamp { get; set; }
    
    public override MessageWriter BuildMessageWriter() {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.RivalCellId);
        messageWriter.Write(StartTimestamp);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static GameStartedNetworkMessage FromMessageReader(MessageReader messageReader) {
        var message = new GameStartedNetworkMessage();
        message.StartTimestamp = messageReader.ReadInt32();
        return message;
    }
}