using Hazel;

public class ScoreUpdatedNetworkMessage: NetworkMessage<ScoreUpdatedNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.ScoreUpdated;
    public int PlayerId { get; set; }
    public int Score { get; set; }
    
    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(PlayerId);
        messageWriter.Write(Score);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        PlayerId = messageReader.ReadInt32();
        Score = messageReader.ReadInt32();
    }
}