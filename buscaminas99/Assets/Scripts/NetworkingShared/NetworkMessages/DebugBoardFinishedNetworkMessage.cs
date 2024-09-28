using Hazel;

namespace NetworkingShared.NetworkMessages
{
    public class DebugBoardFinishedNetworkMessage : NetworkMessage<DebugBoardFinishedNetworkMessage>
    {
        public override NetworkMessageTypes NetworkMessageType => NetworkMessageTypes.DebugBoardFinished;
        
        public int PlayerId { get; set; }

        protected override void BuildMessageWriterImpl(MessageWriter messageWriter) {
            messageWriter.Write(PlayerId);
        }

        protected override void FromMessageReaderImpl(MessageReader messageReader) {
            PlayerId = messageReader.ReadInt32();
        }
    }
}