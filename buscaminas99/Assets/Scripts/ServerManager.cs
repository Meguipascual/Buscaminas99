using Hazel;
using Hazel.Udp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private BoardManager boardManager;
    private UdpConnectionListener connectionListener;
    private List<int> cellIdsToProcess = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>(); 
        connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, 6501));
        connectionListener.NewConnection += PrintConnection;
        connectionListener.Start();
    }
    private void Update()
    {
        foreach (var cellId in cellIdsToProcess)
        {
            var cell = boardManager.GetCell(cellId);
            cell.UseCell();
        }
        cellIdsToProcess.Clear();

    }
    private void PrintConnection(NewConnectionEventArgs args)
    {
        Debug.Log(args.Connection.EndPoint.Address + ":" + args.Connection.EndPoint.Port);
        args.Connection.DataReceived += ProcessMessage;
        
    }
    private void OnDestroy()
    {
        connectionListener.Dispose();
    }
    private void ProcessMessage(DataReceivedEventArgs args) 
    {
        var byteArray = new byte[args.Message.Length]; 
        Array.Copy(args.Message.Buffer, args.Message.Offset, byteArray, 0, byteArray.Length);
        var networkMessage = ByteArrayToNetworkMessage(byteArray);
        Debug.Log(networkMessage.Text);
        cellIdsToProcess.Add(networkMessage.CellId);
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
