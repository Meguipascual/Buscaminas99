using Hazel;

public class GameStartedNetworkMessage : NetworkMessage {
    public long StartTimestamp { get; set; }
    public int GameDurationSeconds { get; set; } 
    
    public override MessageWriter BuildMessageWriter() {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.GameStarted);
        messageWriter.Write(StartTimestamp);
        messageWriter.Write(GameDurationSeconds);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static GameStartedNetworkMessage FromMessageReader(MessageReader messageReader) {
        var message = new GameStartedNetworkMessage();
        message.StartTimestamp = messageReader.ReadInt64();
        message.GameDurationSeconds = messageReader.ReadInt32();
        return message;
    }
}