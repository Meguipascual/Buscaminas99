using Hazel;

public class SeedNetworkMessage : NetworkMessage<SeedNetworkMessage>
{
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.Seed;
    public int Seed { get; set; }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(Seed);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader) {
        Seed = messageReader.ReadInt32();
    }
}
