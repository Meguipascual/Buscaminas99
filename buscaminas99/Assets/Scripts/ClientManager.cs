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
    private Queue<int> cellIdsToProcess = new Queue<int>();
    private Queue<NetworkMessage> unsentMessages = new Queue<NetworkMessage>();
    private BoardManager rivalBoardManager;
    private bool seedMessageProccessed;

    void Start()
    {
        string serverIp = "78.45.45.231";
        // IPAddress.Parse(serverIp) volver a ponerlo en vez de lo de alberto para futuras pruebas locales de server
        FindObjectOfType<NetworkManager>().IsClient = true;
        rivalBoardManager = GameObject.FindGameObjectWithTag(Tags.RivalBoard).GetComponent<BoardManager>();
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(IPAddress.Loopback, 6501));
        clientConnection.DataReceived += HandleMessage;
        clientConnection.ConnectAsync();
    }

    private void Update()
    {
        SendPendingMessages();
        ProcessRivalCellIds();
    }

    private void SendPendingMessages()
    {
        if (clientConnection.State == ConnectionState.Connected)
        {
            while (unsentMessages.Count > 0)
            {
                var messageWriter = unsentMessages.Dequeue().BuildMessageWriter();
                clientConnection.Send(messageWriter);
                messageWriter.Recycle();
            }
        }
    }

    private void ProcessRivalCellIds()
    {
        while (cellIdsToProcess.Count > 0)
        {
            var cellId = cellIdsToProcess.Dequeue();
            if (!seedMessageProccessed)
            {
                rivalBoardManager.GenerateBombs(cellId);
                seedMessageProccessed = true;
                continue;
            }
            var cell = rivalBoardManager.GetCell(cellId);
            cell.UseCell();
        }
        cellIdsToProcess.Clear();
    }

    public void SendCellIdMessage(int cellId)
    {
        if(clientConnection.State != ConnectionState.Connected)
        {
            unsentMessages.Enqueue(new CellIdNetworkMessage { CellId = cellId });
            return;
        }
        var networkMessage = new CellIdNetworkMessage { CellId = cellId }.BuildMessageWriter();
        clientConnection.Send(networkMessage);
        networkMessage.Recycle();
    }

    public void SendSeedMessage(int seed)
    {
        if (clientConnection.State != ConnectionState.Connected)
        {
            unsentMessages.Enqueue(new SeedNetworkMessage { Seed = seed });
            return;
        }
        var networkMessage = new SeedNetworkMessage { Seed = seed }.BuildMessageWriter();
        clientConnection.Send(networkMessage);
        networkMessage.Recycle();
    }

    void HandleMessage(DataReceivedEventArgs args)
    {
        var messageReader = args.Message.ReadMessage();
        switch ((NetworkMessageTypes)messageReader.Tag)
        {
            case NetworkMessageTypes.RivalSeed: 
                var rivalSeedMessage = RivalSeedNetworkMessage.FromMessageReader(messageReader);
                cellIdsToProcess.Enqueue(rivalSeedMessage.Seed);
                Debug.Log($"Rival Seed received: {rivalSeedMessage.Seed}");
                break;
            case NetworkMessageTypes.RivalCellId:
                var rivalCellIdMessage = RivalCellIdNetworkMessage.FromMessageReader(messageReader);
                cellIdsToProcess.Enqueue(rivalCellIdMessage.CellId);
                Debug.Log($"Rival Cell Id received: {rivalCellIdMessage.CellId}");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(messageReader.Tag));
        }
    }
}
