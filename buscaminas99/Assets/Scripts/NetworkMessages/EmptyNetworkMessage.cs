using Hazel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyNetworkMessage : NetworkMessage
{
    public NetworkMessageTypes NetworkMessageType { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageType);
        messageWriter.EndMessage();
        return messageWriter;
    }
}
