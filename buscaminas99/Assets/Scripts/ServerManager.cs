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
            var message = new RivalSeedNetworkMessage { ConnectionId = i, Seed = seedById[i] };
            var messageWriter = message.BuildMessageWriter();
            args.Connection.Send(messageWriter);
            message.BuildMessageWriter().Recycle();
        }

        nextConnectionId++;
    }

    private void OnDestroy()
    {
        connectionListener.Dispose();
    }

    private void ProcessMessage(DataReceivedEventArgs args, int connectionId) 
    {
        var messageReader = args.Message.ReadMessage();
        NetworkMessage networkMessage;
        switch ((NetworkMessageTypes)messageReader.Tag)
        {
            case NetworkMessageTypes.Seed:
                var seedMessage = SeedNetworkMessage.FromMessageReader(messageReader);
                networkMessage = new RivalSeedNetworkMessage { ConnectionId = connectionId, Seed = seedMessage.Seed };
                seedById[connectionId] = seedMessage.Seed;
                Debug.Log($"Rival Seed received: {seedMessage.Seed}");
                break;
            case NetworkMessageTypes.CellId:
                var cellIdMessage = CellIdNetworkMessage.FromMessageReader(messageReader);
                networkMessage = new RivalCellIdNetworkMessage { ConnectionId = connectionId, CellId = cellIdMessage.CellId };
                Debug.Log($"Rival Cell Id received: {cellIdMessage.CellId}");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(messageReader.Tag));
        }
        var messageWriter = networkMessage.BuildMessageWriter();
        for (int i= 0; i < nextConnectionId; i++)
        {
            if(connectionId != i)
            {
                connectionById[i].Send(messageWriter);
            }
        }
        messageWriter.Recycle();
    }

    public void SendResetGameWarning()
    {
        BroadcastEmptyMessage(NetworkMessageTypes.ResetGameWarning);
    }

    public void SendResetGame()
    {
        BroadcastEmptyMessage(NetworkMessageTypes.ResetGame);
    }
    
    private void BroadcastEmptyMessage(NetworkMessageTypes messageType)
    {
        NetworkMessage networkMessage;
        networkMessage = new EmptyNetworkMessage { NetworkMessageType = messageType};
        var messageWriter = networkMessage.BuildMessageWriter();
        for (int i = 0; i < nextConnectionId; i++)
        {
            connectionById[i].Send(messageWriter);
        }
        messageWriter.Recycle();
    }
}
