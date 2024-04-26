using Hazel;

public class EmptyNetworkMessage : NetworkMessage<EmptyNetworkMessage> {
    
    private NetworkMessageTypes _networkMessageType;
    public override NetworkMessageTypes NetworkMessageType => _networkMessageType;

    public void SetNetworkMessageType(NetworkMessageTypes networkMessageType) {
        _networkMessageType = networkMessageType;
    }

    protected override void BuildMessageWriterImpl(MessageWriter messageWriter)
    {
        messageWriter.StartMessage((byte)NetworkMessageType);
    }
}
