using Hazel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
