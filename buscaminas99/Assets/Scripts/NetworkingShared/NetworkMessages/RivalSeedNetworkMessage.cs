using Hazel;

public class RivalSeedNetworkMessage : NetworkMessage
{
    public int Seed { get; set; }
    public int ConnectionId { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.RivalSeed);
        messageWriter.Write(Seed);
        messageWriter.Write(ConnectionId);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static RivalSeedNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new RivalSeedNetworkMessage();
        message.Seed = messageReader.ReadInt32();
        message.ConnectionId = messageReader.ReadInt32();
        return message;
    }
}
