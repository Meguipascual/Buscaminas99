using System.Collections.Generic;
using Hazel;

public class UndoMessageCommandNetworkMessage : NetworkMessage
{
    public int TargetPlayerId { get; set; }
    public List<int> DiscoverCellIds { get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.UndoCommand);
        messageWriter.Write(TargetPlayerId);
        messageWriter.Write(DiscoverCellIds.Count);
        foreach (var cellId in DiscoverCellIds) { messageWriter.Write(cellId); }

        messageWriter.EndMessage();
        return messageWriter;
    }

    public static UndoMessageCommandNetworkMessage FromMessageReader(MessageReader messageReader)
    {

        var message = new UndoMessageCommandNetworkMessage();
        message.TargetPlayerId = messageReader.ReadInt32();
        var sizeDiscoverCells = messageReader.ReadInt32();
        message.DiscoverCellIds = new List<int>();

        for (int i = 0; i < sizeDiscoverCells; i++)
        {
            message.DiscoverCellIds.Add(messageReader.ReadInt32());
        }

        return message;
    }
}
