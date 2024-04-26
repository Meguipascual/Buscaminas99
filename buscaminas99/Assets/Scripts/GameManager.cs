using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
    
    private ClientManager _clientManager;
    
    public TextMeshProUGUI gameOutcomeText;
    private int numberOfBombs;
    private int numberOfCells;
    private int revealedCellsCount;
    private HashSet<int> revealedCellIds = new HashSet<int>();
    private int _startTimestamp;
    
    public bool IsPlayerAlive { get; set; } = true; //Simplifies the get/set structure for a boolean to be accesed by other classes

    private void Start() {
        _clientManager = FindObjectOfType<ClientManager>();

        _clientManager.OnGameStarted += SetStartTimestamp;
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

    private void SetStartTimestamp(int startTimestamp) {
        _startTimestamp = startTimestamp;
    }
}