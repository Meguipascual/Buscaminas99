using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {

    [SerializeField] private bool _overrideIsGameStarted;
    
    private ClientManager _clientManager;
    
    public TextMeshProUGUI gameOutcomeText;
    private int numberOfBombs;
    private int numberOfCells;
    private int revealedCellsCount;
    private HashSet<int> revealedCellIds = new HashSet<int>();
    private long _startTimestamp;
    private int _gameDurationSeconds;

    public bool IsGameStarted => _overrideIsGameStarted || (_startTimestamp > 0 && !IsGameFinished);
    public bool IsGameFinished => _startTimestamp > 0 && DateTime.UtcNow.ToUnixTimeSeconds() >= (_startTimestamp + _gameDurationSeconds);
    
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
            gameOutcomeText.text = $"You Win";
            gameOutcomeText.gameObject.SetActive(true);
            IsPlayerAlive = false;
        }
    }

    private void HandleGameStarted(GameStartedNetworkMessage gameStartedNetworkMessage) {
        _startTimestamp = gameStartedNetworkMessage.StartTimestamp;
        _gameDurationSeconds = gameStartedNetworkMessage.GameDurationSeconds;
    }
}