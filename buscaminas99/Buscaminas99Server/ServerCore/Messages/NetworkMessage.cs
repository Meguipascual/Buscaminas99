using Hazel;

namespace ServerCore.Messages; 

[Serializable]
public abstract class NetworkMessage 
{
    public abstract MessageWriter BuildMessageWriter();
}