using Hazel;

public class GameStartedNetworkMessage : NetworkMessage<GameStartedNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.GameStarted;
    public long StartTimestamp { get; set; }
    public int GameDurationSeconds { get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(StartTimestamp);
        messageWriter.Write(GameDurationSeconds);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        StartTimestamp = messageReader.ReadInt64();
        GameDurationSeconds = messageReader.ReadInt32();
    }
}