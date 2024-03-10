using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class NetworkUtils 
{
    public static byte[] ObjectToByteArray<T>(T obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
    public static NetworkMessage ByteArrayToNetworkMessage(byte[] byteArray)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(byteArray))
        {
            return (NetworkMessage)bf.Deserialize(ms);
        }
    }
}
