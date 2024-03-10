using Hazel;
using Hazel.Udp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ServerManager : MonoBehaviour {
    public const int Port = 6501;
    
    private UdpConnectionListener connectionListener;
    private int nextConnectionId;
    private Dictionary<int, Connection> connectionById = new Dictionary<int, Connection>();
    private Dictionary<int, int> seedById = new Dictionary<int, int>();

    // Start is called before the first frame update
    void Start()
    { 
        connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, Port));
        connectionListener.NewConnection += HandleNewConnection;
        connectionListener.Start();
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
        if (!connectionById.ContainsKey(connectionId))
        {
            Debug.Log($"Message received from connection id {connectionId} not found in dictionary");
            return;
        }

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
        StartCoroutine(ResetGameAfterWaitTime());
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

    private IEnumerator ResetGameAfterWaitTime()
    {
        Debug.Log("It Starts");
        yield return new WaitForSeconds(5);
        Debug.Log("It Waits");
        SendResetGame();
        ClearClientData();
    }

    private void ClearClientData()
    {
        nextConnectionId = 0;
        connectionById.Clear();
        seedById.Clear();
    }

}
