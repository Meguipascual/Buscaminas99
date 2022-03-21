using Hazel.Udp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Hazel;
using System;

public class ClientManager : MonoBehaviour
{
    private UnityUdpClientConnection clientConnection;
    // Start is called before the first frame update
    void Start()
    {
        string ipAlberto = "78.45.45.231";
        // IPAddress.Parse(ipAlberto) volver a ponerlo en vez de lo de alberto para futuras pruebas locales de server
        FindObjectOfType<NetworkManager>().IsClient = true;
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(IPAddress.Loopback , 6501));
        clientConnection.DataReceived += HandleMessage;
        clientConnection.ConnectAsync();
    }
    public new void SendMessage(string msg, int cellId)
    {
        clientConnection.SendBytes(NetworkUtils.ObjectToByteArray (new NetworkMessage() { Text = msg, CellId = cellId}));
    }

    void HandleMessage(DataReceivedEventArgs args)
    {
        var byteArray = new byte[args.Message.Length];
        Array.Copy(args.Message.Buffer, args.Message.Offset, byteArray, 0, byteArray.Length);
        var networkMessage = NetworkUtils.ByteArrayToNetworkMessage(byteArray);
        Debug.Log($"{networkMessage.Text} {networkMessage.CellId} \n connection {networkMessage.ConnectionId}");
    }
}
