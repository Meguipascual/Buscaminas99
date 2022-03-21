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
    private bool seedMessageProccessed;
    private int nextConnectionId;
    private Dictionary<int, Connection> connectionById = new Dictionary<int, Connection>();
    private Dictionary<int, int> seedById = new Dictionary<int, int>();

    // Start is called before the first frame update
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>(); 
        connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, 6501));
        connectionListener.NewConnection += HandleNewConnection;
        connectionListener.Start();
    }

    private void Update()
    {
        foreach (var cellId in cellIdsToProcess)
        {
            if (!seedMessageProccessed)
            {
                boardManager.GenerateBombs(cellId);
                seedMessageProccessed = true;
                continue;
            }
            var cell = boardManager.GetCell(cellId);
            cell.UseCell();
        }
        cellIdsToProcess.Clear();
    }

    private void HandleNewConnection(NewConnectionEventArgs args)
    {
        var newConnectionId = nextConnectionId; 
        Debug.Log(args.Connection.EndPoint.Address + ":" + args.Connection.EndPoint.Port);
        args.Connection.DataReceived += args => ProcessMessage(args, newConnectionId);
        connectionById[newConnectionId] = args.Connection; 
        for (int i = 0; i < nextConnectionId; i++)
        {
            var message = new NetworkMessage { ConnectionId = i, Seed = seedById[i] };
            args.Connection.SendBytes(NetworkUtils.ObjectToByteArray(message));
        }
        nextConnectionId++;
    }

    private void OnDestroy()
    {
        connectionListener.Dispose();
    }

    private void ProcessMessage(DataReceivedEventArgs args, int connectionId) 
    {
        var byteArray = new byte[args.Message.Length];
        Array.Copy(args.Message.Buffer, args.Message.Offset, byteArray, 0, byteArray.Length);
        var networkMessage = NetworkUtils.ByteArrayToNetworkMessage(byteArray);
        Debug.Log(networkMessage.Text + " " + networkMessage.CellId);
        //cellIdsToProcess.Add(networkMessage.CellId);
        Debug.Log($"Connection id: {connectionId}");
        if (networkMessage.Seed.HasValue)
        {
            seedById[connectionId] = networkMessage.Seed.Value;
        }
        networkMessage.ConnectionId = connectionId;
        byteArray = NetworkUtils.ObjectToByteArray(networkMessage);

        for(int i= 0; i < nextConnectionId; i++)
        {
            if(connectionId != i)
            {
                connectionById[i].SendBytes(byteArray);
            }
        }
    }

    
}
