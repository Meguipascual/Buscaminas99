using Hazel;
using Hazel.Udp;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private UdpConnectionListener connectionListener;
    // Start is called before the first frame update
    void Start()
    {
        connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, 6501));
        connectionListener.NewConnection += PrintConnection;
        connectionListener.Start();
    }
    private void PrintConnection(NewConnectionEventArgs args)
    {
        Debug.Log(args.Connection.EndPoint.Address + ":" + args.Connection.EndPoint.Port);
        args.Connection.DataReceived += PrintMessage;
    }
    private void OnDestroy()
    {
        connectionListener.Dispose();
    }
    private void PrintMessage(DataReceivedEventArgs args) 
    {
        var byteArray = new byte[args.Message.Length]; 
        Array.Copy(args.Message.Buffer, args.Message.Offset, byteArray, 0, byteArray.Length);
        var networkMessage = ByteArrayToNetworkMessage(byteArray);
        Debug.Log(networkMessage.Text); 
    }
    NetworkMessage ByteArrayToNetworkMessage(byte[] byteArray)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(byteArray))
        {
            return (NetworkMessage)bf.Deserialize(ms);
        }
    }
}
