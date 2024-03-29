using Hazel.Udp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Hazel;
using System;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    private UnityUdpClientConnection clientConnection;
    private Queue<int> cellIdsToProcess = new Queue<int>();
    private Queue<NetworkMessage> unsentMessages = new Queue<NetworkMessage>();
    private BoardManager rivalBoardManager;
    private bool mustRestartScene;
    private int? rivalSeed;

    void Start() {
        var ipAddress = IPAddress.Loopback; // For localhost, replace with GetIPAddress(x.x.x.x) and the proper ip for online tests
        FindObjectOfType<NetworkManager>().IsClient = true;
        rivalBoardManager = GameObject.FindGameObjectWithTag(Tags.RivalBoard).GetComponent<BoardManager>();
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(ipAddress, ServerManager.Port));
        clientConnection.DataReceived += HandleMessage;
        clientConnection.ConnectAsync();
    }

    // DO NOT DELETE - We'll use this when we want to use real online multiplayer
    private IPAddress GetIPAddress(string serverIp) {
        return IPAddress.Parse(serverIp);
    }

    private void Update()
    {
        if (rivalSeed.HasValue) {
            rivalBoardManager.Seed = rivalSeed.Value;
            rivalBoardManager.GenerateBombs();
            rivalSeed = null;
        }

        SendPendingMessages();
        ProcessRivalCellIds();

        if (mustRestartScene)
        {
            clientConnection.Disconnect("Game Finished");
            clientConnection.Dispose();
            SceneManager.LoadScene(SceneNames.Client);
        }
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
                rivalSeed = rivalSeedMessage.Seed;
                Debug.Log($"Rival Seed received: {rivalSeedMessage.Seed}");
                break;
            case NetworkMessageTypes.RivalCellId:
                var rivalCellIdMessage = RivalCellIdNetworkMessage.FromMessageReader(messageReader);
                cellIdsToProcess.Enqueue(rivalCellIdMessage.CellId);
                Debug.Log($"Rival Cell Id received: {rivalCellIdMessage.CellId}");
                break;
            case NetworkMessageTypes.ResetGame:
                Debug.Log($"Reset Game message received");
                mustRestartScene = true;
                break;
            case NetworkMessageTypes.ResetGameWarning:
                Debug.Log($"Reset Game Warning message received");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(messageReader.Tag));
        }
    }
}
