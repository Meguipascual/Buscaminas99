using Hazel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIdNetworkMessage : NetworkMessage
{
    public int CellId { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.CellId);
        messageWriter.Write(CellId);
        messageWriter.EndMessage();
        return messageWriter;
    }

    public static CellIdNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        var message = new CellIdNetworkMessage();
        message.CellId = messageReader.ReadInt32();
        return message;
    }
}
