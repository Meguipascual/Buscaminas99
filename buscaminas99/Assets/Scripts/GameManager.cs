using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;


public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI gameOutcomeText;
    private int numberOfBombs;
    private int numberOfCells;
    private int revealedCellsCount;
    HashSet<int> revealedCellIds = new HashSet<int>();
    public bool IsPlayerAlive { get; set; } = true; //Simplifies the get/set structure for a boolean to be accesed by other classes

    public void Reset()
    {
        SceneManager.LoadScene("MineSweeper");
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
}