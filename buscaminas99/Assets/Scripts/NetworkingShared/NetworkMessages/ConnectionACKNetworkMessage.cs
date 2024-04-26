using Hazel;
using System.Collections;
using System.Collections.Generic;

public class ConnectionACKNetworkMessage : NetworkMessage
{
    public int PlayerId {  get; set; }

    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.ConnectionACK);
        messageWriter.Write(PlayerId);

        messageWriter.EndMessage();
        return messageWriter;
    }

    public static ConnectionACKNetworkMessage FromMessageReader(MessageReader messageReader)
    {

        var message = new ConnectionACKNetworkMessage();
        message.PlayerId = messageReader.ReadInt32();
        
        return message;
    }
}
