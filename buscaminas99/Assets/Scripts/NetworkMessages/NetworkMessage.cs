using Hazel;
using System;

[Serializable]
public abstract class NetworkMessage 
{
    public abstract MessageWriter BuildMessageWriter();
}
