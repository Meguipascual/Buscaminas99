using Hazel;

public class RivalSeedNetworkMessage : NetworkMessage<RivalSeedNetworkMessage>
{
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.RivalSeed;
    public int Seed { get; set; }
    public int ConnectionId { get; set; }
    
    protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
        messageWriter.Write(Seed);
        messageWriter.Write(ConnectionId);
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader)
    {
        Seed = messageReader.ReadInt32();
        ConnectionId = messageReader.ReadInt32();
    }
}
