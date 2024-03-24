using Hazel;

namespace ServerCore.Messages; 

/// <summary>
/// Message used to relay the seed used to randomly generate the player's board.
/// </summary>
public class SeedNetworkMessage : NetworkMessage
{
    public int Seed { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.Seed);
        messageWriter.Write(Seed);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static SeedNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new SeedNetworkMessage();
        message.Seed = messageReader.ReadInt32();
        return message;
    }
}