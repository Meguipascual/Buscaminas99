using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject flag;
    private GameManager gameManager;
    private BoardManager boardManager;
    private ClientManager clientManager;
    public TextMesh number;
    private int id;

    private Vector3[] eightVariations = new Vector3[8];
    private Vector3[] fourVariations = new Vector3[4];

    public int Id => id;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        clientManager = FindObjectOfType<ClientManager>();
        FillVariations();
        id = boardManager.GenerateId(gameObject.transform.position);
        boardManager.RegisterCell(this);
    }

    public void SetBoardManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    private void OnMouseDown()
    {
        if (gameManager.IsPlayerAlive && !boardManager.IsRivalBoard)
        {
            clientManager.SendCellIdMessage(id);
            UseCell();
        }
    }

    public void UseCell()
    {
        if (boardManager.BombExists(gameObject.transform.position))
        {
            DestroyCell();
            if (!boardManager.IsRivalBoard)
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                gameManager.IsPlayerAlive = false;
                gameManager.gameOutcomeText.gameObject.SetActive(true);
            }
        }
        else
        {
            DisplayBombsNear();
            Debug.Log("It's aliiiiiiiive");
            boardManager.RevealNeighbourCells(this);
        }
    }  

    
    /// <summary>
    /// Puts or removes flags when right click
    /// </summary>
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (gameManager.IsPlayerAlive)
            {
                Instantiate(flag, gameObject.transform.position, transform.rotation);
                //controlar que solo se pueda crear una flag haciendo que si le vuelve a dar la elimine en vez de crear otra
            }
        }
    }

    /// <summary>
    /// Fills the variations of position needed by DisplayBombsNear
    /// </summary>
    private void FillVariations()
    {
        eightVariations[0] = new Vector3(-boardManager.CellSize, 0);
        eightVariations[1] = new Vector3(-boardManager.CellSize, boardManager.CellSize);
        eightVariations[2] = new Vector3(-boardManager.CellSize, -boardManager.CellSize);
        eightVariations[3] = new Vector3(0, -boardManager.CellSize);
        eightVariations[4] = new Vector3(0, boardManager.CellSize);
        eightVariations[5] = new Vector3(boardManager.CellSize, -boardManager.CellSize);
        eightVariations[6] = new Vector3(boardManager.CellSize, 0);
        eightVariations[7] = new Vector3(boardManager.CellSize, boardManager.CellSize);
    }

    /// <summary>
    /// Displays a number that represents how many bombs are nearby
    /// </summary>
    public void DisplayBombsNear()
    {
        var position = gameObject.transform.position;
        var num = 0;
        if (boardManager.BombExists(position))
        {
            return;
        }

        foreach (var neighbourPosition in CalculateEightNeighbourCellPositions())
        {
            if (boardManager.BombExists(neighbourPosition)) {
                num++;
            }
        }

        if (num == 0)
        {
            boardManager.RevealNeighbourCells(this);
        }
        else
        {
            number.text = num.ToString();

            number.gameObject.SetActive(true);
        }
        DestroyCell();
    }

    private void DestroyCell()
    {
        Destroy(this.gameObject);

        if (!boardManager.IsRivalBoard)
        {
            gameManager.TrackCellRevealed(Id);
        }
    }

    public IEnumerable<Vector3> CalculateEightNeighbourCellPositions() {
        var position = gameObject.transform.position;
        for (int i = 0; i < 8; i++)
        {
            var auxX = position.x + eightVariations[i].x;
            var auxY = position.y + eightVariations[i].y;
            if (auxX >= (boardManager.BoardCenterPosition.x - boardManager.BoardHalf) - GlobalConstants.FloatPrecision
                && auxX <= (boardManager.BoardCenterPosition.x + boardManager.BoardHalf) + GlobalConstants.FloatPrecision
                && auxY >= (boardManager.BoardCenterPosition.y - boardManager.BoardHalf) - GlobalConstants.FloatPrecision
                && auxY <= (boardManager.BoardCenterPosition.y + boardManager.BoardHalf) + GlobalConstants.FloatPrecision)
            {
                yield return position + eightVariations[i];
            }
        }
    }
}
