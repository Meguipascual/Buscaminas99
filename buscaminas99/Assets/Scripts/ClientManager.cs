using Hazel.Udp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientManager : MonoBehaviour
{
    private UnityUdpClientConnection clientConnection;
    // Start is called before the first frame update
    void Start()
    {
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(IPAddress.Loopback, 6501));
        clientConnection.ConnectAsync();
    }
    public new void SendMessage(string msg)
    {
        clientConnection.SendBytes(ObjectToByteArray (new NetworkMessage() { Text = msg}));
    }
    byte[] ObjectToByteArray<T>(T obj)
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
}
