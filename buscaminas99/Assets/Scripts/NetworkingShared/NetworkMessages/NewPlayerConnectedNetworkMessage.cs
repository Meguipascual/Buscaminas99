using Hazel;
using System.Collections;
using System.Collections.Generic;

public class NewPlayerConnectedNetworkMessage : NetworkMessage
{
    public int PlayerId { get; set; }


    public override MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageTypes.NewPlayerConnected);
        messageWriter.Write(PlayerId);

        messageWriter.EndMessage();
        return messageWriter;
    }

    public static NewPlayerConnectedNetworkMessage FromMessageReader(MessageReader messageReader)
    {

        var message = new NewPlayerConnectedNetworkMessage();
        message.PlayerId = messageReader.ReadInt32();

        return message;
    }
}
