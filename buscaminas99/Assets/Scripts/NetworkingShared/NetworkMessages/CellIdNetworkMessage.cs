using Hazel;
using System.Collections.Generic;

public class CellIdNetworkMessage : NetworkMessage
{
    public int CellId { get; set; }
    public List <int> DiscoverCellIds {get; set;}

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.CellId);
        messageWriter.Write(CellId);
        messageWriter.Write(DiscoverCellIds.Count);
        foreach (var cellId in DiscoverCellIds) { messageWriter.Write(cellId); }

        messageWriter.EndMessage();
        return messageWriter;
    }

    public static CellIdNetworkMessage FromMessageReader(MessageReader messageReader)
    {
        
        var message = new CellIdNetworkMessage();
        message.CellId = messageReader.ReadInt32();
        var sizeDiscoverCells = messageReader.ReadInt32();
        message.DiscoverCellIds = new List <int>();

        for (int i = 0; i < sizeDiscoverCells; i++)
        {
            message.DiscoverCellIds.Add(messageReader.ReadInt32());
        }
        
        return message;
    }
}
