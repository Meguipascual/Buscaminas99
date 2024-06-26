using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField] private bool _overrideIsGameActive;
    [SerializeField] private TextMeshProUGUI _gameOutcomeText;
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private Button _resetBoardButton;
    [SerializeField] private TMP_Text _boardFinishedText;
    
    private ClientManager _clientManager;
    private BoardManager _localBoardManager;
    
    private int numberOfBombs;
    private int numberOfCells;
    private HashSet<int> revealedCellIds = new HashSet<int>();
    private long _startTimestamp;
    private int _gameDurationSeconds;

    public bool IsGameActive => _overrideIsGameActive || (IsGameStarted && !IsGameFinished);
    private bool IsGameStarted => _startTimestamp > 0;
    private bool IsGameFinished => DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= (_startTimestamp + _gameDurationSeconds);
    
    public bool IsPlayerAlive { get; set; } = true; //Simplifies the get/set structure for a boolean to be accesed by other classes

    private void Start() {
        _clientManager = FindObjectOfType<ClientManager>();
        _clientManager.OnGameStarted += HandleGameStarted;

        var boardManagers = FindObjectsOfType<BoardManager>();
        _localBoardManager = boardManagers.Single(boardManager => !boardManager.IsRivalBoard);
        
        _resetBoardButton.gameObject.SetActive(false);
        _boardFinishedText.gameObject.SetActive(false);
    }

    private void OnEnable() {
        if (_clientManager != null) {
            _clientManager.OnGameStarted += HandleGameStarted;
        }
    }

    private void OnDisable() {
        _clientManager.OnGameStarted -= HandleGameStarted;
    }

    public void Reset()
    {
        if (_clientManager.IsOnline) {
            Debug.Log("Is online, requesting server reset");
            _clientManager.RequestServerReset();
        }
        else {
            Debug.Log("Is offline, restarting scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void Rewind()
    {
        if (_clientManager.IsOnline)
        {
            Debug.Log("Is online, requesting server to undo previous play");
            _clientManager.SendUndoPlayMessage();
        }
        else
        {
            Debug.Log("Is offline, undoing previous play");
        }
    }

    public void SetBoardFeatures(int numOfBombs, int numOfCells)
    {
        numberOfBombs = numOfBombs; 
        numberOfCells = numOfCells;
    }

    public void TrackCellRevealed(int cellId)
    {
        if (!IsPlayerAlive || revealedCellIds.Contains(cellId))
        {
            return;
        }

        revealedCellIds.Add(cellId);
        if (revealedCellIds.Count == (numberOfCells - numberOfBombs))
        {
            IsPlayerAlive = false;
            _gameOutcomeText.text = $"You Win";
            _gameOutcomeText.gameObject.SetActive(true);
            _resetBoardButton.gameObject.SetActive(true);
            
            _clientManager.NotifyBoardFinished();
        }
    }

    public void KillPlayer() {
        Debug.Log("GameOver, te has murido muy fuerte");
        IsPlayerAlive = false;
        _gameOutcomeText.gameObject.SetActive(true);
        _clientManager.NotifyPlayerEliminated();
    }

    public void StartNewBoard() {
        IsPlayerAlive = true;
        revealedCellIds.Clear();
        _gameOutcomeText.gameObject.SetActive(false);
        _localBoardManager.Reset();
        _resetBoardButton.gameObject.SetActive(false);
        _boardFinishedText.gameObject.SetActive(false);
    }

    private void HandleGameStarted(GameStartedNetworkMessage gameStartedNetworkMessage) {
        _startTimestamp = gameStartedNetworkMessage.StartTimestamp;
        _gameDurationSeconds = gameStartedNetworkMessage.GameDurationSeconds;
        _gameTimer.StartTimer(
            gameStartedNetworkMessage.StartTimestamp, 
            gameStartedNetworkMessage.GameDurationSeconds);
    }

    public void DebugFinishBoard() {
        _localBoardManager.DebugFinishBoard();
    }

    public void DisplayBoardFinishedText(int points) {
        _boardFinishedText.gameObject.SetActive(true);
        _boardFinishedText.text = $"Board finished. Points: {points}";
    }
}