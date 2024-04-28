using Hazel.Udp;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Hazel;
using System;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    private UnityUdpClientConnection clientConnection;
    private Queue<int> cellIdsToProcess = new Queue<int>();
    private Queue<INetworkMessage> unsentMessages = new Queue<INetworkMessage>();
    private Queue<INetworkMessage> pendingReceivedMessages = new Queue<INetworkMessage>();
    private BoardManager rivalBoardManager;
    private bool mustRestartScene;
    private int? rivalSeed;
    private int _playerId;
    private Dictionary<int, Player> _playersById = new Dictionary<int, Player>();
    
    public delegate void GameStartedDelegate(GameStartedNetworkMessage gameStartedNetworkMessage);
    public event GameStartedDelegate OnGameStarted;

    public bool IsOnline => clientConnection.State == ConnectionState.Connected;

    void Start() {
        var ipAddress = IPAddress.Loopback; // For localhost, replace with GetIPAddress(x.x.x.x) and the proper ip for online tests
        FindObjectOfType<NetworkManager>().IsClient = true;
        rivalBoardManager = GameObject.FindGameObjectWithTag(Tags.RivalBoard).GetComponent<BoardManager>();
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(ipAddress, 6501));
        clientConnection.DataReceived += HandleMessage;
        clientConnection.ConnectAsync();
        _playersById = new Dictionary<int, Player>();
    }

    // DO NOT DELETE - We'll use this when we want to use real online multiplayer
    private IPAddress GetIPAddress(string serverIp) {
        return IPAddress.Parse(serverIp);
    }

    private void Update()
    {
        if (rivalSeed.HasValue) {
            rivalBoardManager.Seed = rivalSeed.Value;
            rivalBoardManager.Reset();
            rivalBoardManager.GenerateBombs();
            rivalSeed = null;
        }

        SendPendingMessages();
        ProcessRivalCellIds();
        
        HandlePendingReceivedMessages();

        if (mustRestartScene)
        {
            clientConnection.Disconnect("Game Finished");
            clientConnection.Dispose();
            SceneManager.LoadScene(SceneNames.Game);
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

    public void SendCellIdMessage(int cellId, List<int> discoverCellIds)
    {
        SendMessage(new CellIdNetworkMessage { CellId = cellId, DiscoverCellIds = discoverCellIds});
    }

    public void SendSeedMessage(int seed)
    {
        SendMessage(new SeedNetworkMessage { Seed = seed });
    }

    public void RequestServerReset() {
        SendEmptyMessage(NetworkMessageTypes.ResetServer);
    }

    public void NotifyBoardFinished() {
        SendEmptyMessage(NetworkMessageTypes.BoardFinished);
    }

    private void SendEmptyMessage(NetworkMessageTypes messageType) {
        var message = new EmptyNetworkMessage();
        message.SetNetworkMessageType(messageType);
        SendMessage(message);
    }

    public void SendUndoPlayMessage()
    {
        var targetPLayerId = 1 - _playerId;
        SendMessage(new UndoPlayNetworkMessage() { TargetPlayerId = targetPLayerId });
    }

    private void SendMessage(INetworkMessage networkMessage) {
        if (clientConnection.State != ConnectionState.Connected)
        {
            unsentMessages.Enqueue(networkMessage);
            return;
        }
        var networkMessageWriter = networkMessage.BuildMessageWriter();
        clientConnection.Send(networkMessageWriter);
        networkMessageWriter.Recycle();
    }

    private void HandleMessage(DataReceivedEventArgs args)
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
            case NetworkMessageTypes.ConnectionACK:
                var connectionACKMessage = ConnectionACKNetworkMessage.FromMessageReader( messageReader);
                _playerId = connectionACKMessage.PlayerId;
                Debug.Log($"Player ConnectedACK Id Received: {_playerId}");
                break;
            case NetworkMessageTypes.NewPlayerConnected: 
                var newPlayerConnectedMessage = NewPlayerConnectedNetworkMessage.FromMessageReader( messageReader);
                Debug.Log($"Player Connected Id Received: {newPlayerConnectedMessage.PlayerId}");
                var player = new Player();
                player.PlayerId = newPlayerConnectedMessage.PlayerId;
                _playersById.Add(player.PlayerId, player);
                break;
            case NetworkMessageTypes.UndoCommand:
                var undoCommandMessage = UndoMessageCommandNetworkMessage.FromMessageReader( messageReader);
                Debug.Log($"Undo Command message received");
                break;
            case NetworkMessageTypes.GameStarted:
                var gameStartedMessage = GameStartedNetworkMessage.FromMessageReader(messageReader);
                pendingReceivedMessages.Enqueue(gameStartedMessage);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(messageReader.Tag));
        }
    }

    /// <summary>
    /// Used to process messages in the main thread (called from Update),
    /// mainly for those that will update elements in the UI.
    /// </summary>
    private void HandlePendingReceivedMessages() {
        while (pendingReceivedMessages.Count > 0) {
            HandlePendingReceivedMessage(pendingReceivedMessages.Dequeue());
        }
    }

    private void HandlePendingReceivedMessage(INetworkMessage networkMessage) {
        switch (networkMessage.NetworkMessageType)
        {
            case NetworkMessageTypes.GameStarted:
                var gameStartedMessage = (GameStartedNetworkMessage)networkMessage;
                Debug.Log($"Setting start time to {gameStartedMessage.StartTimestamp}");
                OnGameStarted?.Invoke(gameStartedMessage);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(networkMessage.NetworkMessageType));
        }
    }
}
