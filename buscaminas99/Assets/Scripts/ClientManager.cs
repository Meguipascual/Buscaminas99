using Hazel.Udp;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Hazel;
using System;
using System.Collections.Concurrent;
using System.Linq;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour {
    [SerializeField] private TMP_Text _playerIdText;
    [SerializeField] private TMP_Text _scoresText;

    private GameManager _gameManager;
    
    private UnityUdpClientConnection clientConnection;
    private Queue<int> cellIdsToProcess = new Queue<int>();
    private Queue<INetworkMessage> unsentMessages = new Queue<INetworkMessage>();
    private ConcurrentQueue<INetworkMessage> pendingReceivedMessages = new ConcurrentQueue<INetworkMessage>();
    private BoardManager rivalBoardManager;
    private bool mustRestartScene;
    private int? rivalSeed;
    private int _playerId = -1;
    private Dictionary<int, Player> _playersById = new Dictionary<int, Player>();
    
    public delegate void GameStartedDelegate(GameStartedNetworkMessage gameStartedNetworkMessage);
    public event GameStartedDelegate OnGameStarted;

    public bool IsOnline => clientConnection.State == ConnectionState.Connected;

    void Start() {
        _gameManager = FindObjectOfType<GameManager>();
        
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

    public void NotifyPlayerEliminated() {
        SendEmptyMessage(NetworkMessageTypes.PlayerEliminated);
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
                var connectionACKMessage = ConnectionACKNetworkMessage.FromMessageReader(messageReader);
                pendingReceivedMessages.Enqueue(connectionACKMessage);
                break;
            case NetworkMessageTypes.NewPlayerConnected: 
                var newPlayerConnectedMessage = NewPlayerConnectedNetworkMessage.FromMessageReader(messageReader);
                pendingReceivedMessages.Enqueue(newPlayerConnectedMessage);
                break;
            case NetworkMessageTypes.UndoCommand:
                var undoCommandMessage = UndoMessageCommandNetworkMessage.FromMessageReader(messageReader);
                Debug.Log($"Undo Command message received");
                break;
            case NetworkMessageTypes.GameStarted:
                var gameStartedMessage = GameStartedNetworkMessage.FromMessageReader(messageReader);
                pendingReceivedMessages.Enqueue(gameStartedMessage);
                break;
            case NetworkMessageTypes.ScoreUpdated:
                var scoreUpdatedMessage = ScoreUpdatedNetworkMessage.FromMessageReader(messageReader);
                pendingReceivedMessages.Enqueue(scoreUpdatedMessage);
                break;
            case NetworkMessageTypes.GameEnded:
                var gameEndedMessage = GameEndedNetworkMessage.FromMessageReader(messageReader);
                Debug.Log($"Game ended message received with {gameEndedMessage.Scores.Count} scores");
                pendingReceivedMessages.Enqueue(gameEndedMessage);
                break;
            case NetworkMessageTypes.RivalEliminated:
                var rivalEliminatedMessage = RivalEliminatedNetworkMessage.FromMessageReader(messageReader);
                Debug.Log($"Rival {rivalEliminatedMessage.PlayerId} eliminated");
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
            if (pendingReceivedMessages.TryDequeue(out var message)) {
                HandlePendingReceivedMessage(message);
            }
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
            case NetworkMessageTypes.NewPlayerConnected:
                var newPlayerConnectedMessage = (NewPlayerConnectedNetworkMessage)networkMessage;
                Debug.Log($"Player Connected Id Received: {newPlayerConnectedMessage.PlayerId}");
                AddPlayer(newPlayerConnectedMessage.PlayerId);
                break;
            case NetworkMessageTypes.ConnectionACK:
                var connectionACKMessage = (ConnectionACKNetworkMessage)networkMessage;
                _playerId = connectionACKMessage.PlayerId;
                AddPlayer(connectionACKMessage.PlayerId);
                _playerIdText.text = $"Player {_playerId}";
                Debug.Log($"Player ConnectedACK Id Received: {_playerId}");
                break;
            case NetworkMessageTypes.ScoreUpdated:
                var scoreUpdatedMessage = (ScoreUpdatedNetworkMessage)networkMessage;

                if (scoreUpdatedMessage.PlayerId == _playerId) {
                    _gameManager.DisplayBoardFinishedText(scoreUpdatedMessage.Score - _playersById[scoreUpdatedMessage.PlayerId].Score);
                }
                
                _playersById[scoreUpdatedMessage.PlayerId].Score = scoreUpdatedMessage.Score;
                UpdateScoresText();
                Debug.Log($"Score updated for player {scoreUpdatedMessage.PlayerId}: {scoreUpdatedMessage.Score}");
                break;
            case NetworkMessageTypes.GameEnded:
                var gameEndedMessage = (GameEndedNetworkMessage)networkMessage;
                foreach (var scoreDto in gameEndedMessage.Scores) {
                    _playersById[scoreDto.PlayerId].Score = scoreDto.Score;
                }
                SetEndOfGameScoresText();
                break;
            default: throw new ArgumentOutOfRangeException(nameof(networkMessage.NetworkMessageType));
        }
    }

    private Player AddPlayer(int connectionId) {
        var player = new Player();
        player.PlayerId = connectionId;
        _playersById.Add(player.PlayerId, player);
        UpdateScoresText();
        return player;
    }

    private void UpdateScoresText() {
        var scoresText = string.Empty;
        var playersSortedByScore = _playersById.Values.OrderByDescending(p => p.Score);
        foreach (var player in playersSortedByScore) {
            scoresText += $"Player {player.PlayerId} - Score: {player.Score}{Environment.NewLine}";
        }
        _scoresText.text = scoresText;
    }

    private void SetEndOfGameScoresText() {
        UpdateScoresText();
        _scoresText.text = $"(END OF GAME) {Environment.NewLine}{_scoresText.text}";
        _scoresText.fontStyle = FontStyles.Bold;
    }
}
