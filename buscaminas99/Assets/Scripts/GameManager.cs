using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {

    [SerializeField] private bool _overrideIsGameActive;
    [SerializeField] private TextMeshProUGUI _gameOutcomeText;
    [SerializeField] private GameTimer _gameTimer;
    
    private ClientManager _clientManager;
    
    private int numberOfBombs;
    private int numberOfCells;
    private int revealedCellsCount;
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

    public void SetBoardFeatures(int numOfBombs, int numOfCells)
    {
        numberOfBombs = numOfBombs; 
        numberOfCells = numOfCells;
    }

    public void TrackCellRevealed(int cellId)
    {
        if (revealedCellIds.Contains(cellId))
        {
            return;
        }

        revealedCellIds.Add(cellId);
        revealedCellsCount++;
        if (revealedCellsCount == (numberOfCells - numberOfBombs))
        {
            _gameOutcomeText.text = $"You Win";
            _gameOutcomeText.gameObject.SetActive(true);
            IsPlayerAlive = false;
        }
    }

    public void KillPlayer() {
        Debug.Log("GameOver, te has murido muy fuerte");
        IsPlayerAlive = false;
        _gameOutcomeText.gameObject.SetActive(true);
    }

    private void HandleGameStarted(GameStartedNetworkMessage gameStartedNetworkMessage) {
        _startTimestamp = gameStartedNetworkMessage.StartTimestamp;
        _gameDurationSeconds = gameStartedNetworkMessage.GameDurationSeconds;
        _gameTimer.StartTimer(gameStartedNetworkMessage.GameDurationSeconds);
    }
}