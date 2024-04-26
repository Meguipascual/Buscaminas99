using Hazel;
using System;

public interface INetworkMessage {
    NetworkMessageTypes NetworkMessageType { get; }
    MessageWriter BuildMessageWriter();
}

[Serializable]
public abstract class NetworkMessage<T> : INetworkMessage where T : NetworkMessage<T>, new() {
    
    public abstract NetworkMessageTypes NetworkMessageType { get; } 

    public MessageWriter BuildMessageWriter()
    {
        var messageWriter = MessageWriter.Get();
        messageWriter.StartMessage((byte)NetworkMessageType);
        
        BuildMessageWriterImpl(messageWriter);

        messageWriter.EndMessage();
        return messageWriter;
    }
    
    protected abstract void BuildMessageWriterImpl(MessageWriter messageWriter);

    public static T FromMessageReader(MessageReader messageReader)
    {
        var message = new T();
        message.FromMessageReaderImpl(messageReader);
        return message;
    }

    protected virtual void FromMessageReaderImpl(MessageReader messageReader) { }
}
