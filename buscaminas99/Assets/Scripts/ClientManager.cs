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
    private BoardManager rivalBoardManager;
    private bool seedMessageProccessed;
    private int messagesProcessedCount;
    // Start is called before the first frame update
    void Start()
    {
        string serverIp = "78.45.45.231";
        // IPAddress.Loopback volver a ponerlo en vez de lo de alberto para futuras pruebas locales de server
        FindObjectOfType<NetworkManager>().IsClient = true;
        rivalBoardManager = GameObject.FindGameObjectWithTag(Tags.RivalBoard).GetComponent<BoardManager>();
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(IPAddress.Parse(serverIp), 6501));
        clientConnection.DataReceived += HandleMessage;
        clientConnection.ConnectAsync();
    }
    private void Update()
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
    public new void SendMessage(string msg, int cellId)
    {
        clientConnection.SendBytes(NetworkUtils.ObjectToByteArray (new NetworkMessage() { Text = msg, CellId = cellId}));
    }
    public new void SendSeedMessage(string msg, int seed)
    {
        clientConnection.SendBytes(NetworkUtils.ObjectToByteArray(new NetworkMessage() { Text = msg, Seed = seed }));
    }

    void HandleMessage(DataReceivedEventArgs args)
    {
        var byteArray = new byte[args.Message.Length];
        Array.Copy(args.Message.Buffer, args.Message.Offset, byteArray, 0, byteArray.Length);
        var networkMessage = NetworkUtils.ByteArrayToNetworkMessage(byteArray);
        Debug.Log($"{networkMessage.Text} {networkMessage.CellId} \n connection: {networkMessage.ConnectionId}  seed: {networkMessage.Seed}");
        if (messagesProcessedCount == 0)
        {
            cellIdsToProcess.Enqueue(networkMessage.Seed.Value);
        }
        else
        {
            cellIdsToProcess.Enqueue(networkMessage.CellId);
        }
        messagesProcessedCount++;
    }
}
