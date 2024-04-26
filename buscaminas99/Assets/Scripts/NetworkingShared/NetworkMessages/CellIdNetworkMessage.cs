using Hazel;
using System.Collections.Generic;

public class CellIdNetworkMessage : NetworkMessage<CellIdNetworkMessage> {
    public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.CellId;
    public int CellId { get; set; }
    public List <int> DiscoverCellIds {get; set;}

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter)
    {
        messageWriter.Write(CellId);
        messageWriter.Write(DiscoverCellIds.Count);
        foreach (var cellId in DiscoverCellIds) { messageWriter.Write(cellId); }
    }

    protected override void FromMessageReaderImpl(MessageReader messageReader)
    {
        CellId = messageReader.ReadInt32();
        var sizeDiscoverCells = messageReader.ReadInt32();
        DiscoverCellIds = new List <int>();

        for (int i = 0; i < sizeDiscoverCells; i++)
        {
            DiscoverCellIds.Add(messageReader.ReadInt32());
        }
    }
}
