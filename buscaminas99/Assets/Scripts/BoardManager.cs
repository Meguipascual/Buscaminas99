using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

using Random = System.Random;


public class BoardManager : MonoBehaviour
{

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] Transform boardCenter;
    private GameManager gameManager;
    private readonly int numberOfBombs = 18;//Amount of bombs to create
    private readonly int numberOfColumns = 11;//Table columns
    private readonly int numberOfRows = 11;//Table rows
    private Random random;
    List<int> cellIdsWithBombs = new List<int>();//Ids' list that have a bomb 
    List<int> allCellIds = new List<int>();//List of all ids 
    Dictionary<int, Cell> cellById = new Dictionary<int, Cell>();//Dictionary that relates an ID with its Cell
    public Vector2 BoardCenterPosition { get; private set; }
    public bool IsRivalBoard => gameObject.CompareTag(Tags.RivalBoard);
    public float Scale { get; private set; }
    public float CellSize => Scale * 5;
    public float BoardHalf => CellSize * (numberOfColumns - 1) / 2;
    public bool AreBombsGenerated { get; set; }
    public int Seed { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        BoardCenterPosition = boardCenter.position;
        Scale = transform.localScale.x;
        GenerateCells();
        if (FindObjectOfType<NetworkManager>().IsClient && !IsRivalBoard) 
        { 
            var seedRandom = new Random();
            Seed=seedRandom.Next();
            
            FindObjectOfType<ClientManager>().SendSeedMessage(Seed);
        }

        if (!IsRivalBoard)
        {
            gameManager.SetBoardFeatures(numberOfBombs, numberOfColumns * numberOfRows);
        }
    }

    public void RegisterCell(Cell cell)
    {
        cellById[cell.Id] = cell;
    }

    public Cell GetCell(int cellId)
    {
        return cellById[cellId];
    }

    //Generates cells all over the table
    void GenerateCells()
    {
        var initialPosition = new Vector3(BoardCenterPosition.x - BoardHalf, BoardCenterPosition.y + BoardHalf, -0.4f);
        var incrementX = new Vector3(CellSize, 0, 0);
        var decrementY = new Vector3(0, CellSize, 0);
        var initialPosX = BoardCenterPosition.x - BoardHalf;

        for (int i = 0; i < numberOfColumns; i++)
        {
            for (int j = 0; j < numberOfRows; j++)
            {
                var cell = Instantiate(cellPrefab, initialPosition, cellPrefab.transform.rotation).GetComponentInChildren<Cell>();
                cell.SetBoardManager(this);
                cell.transform.localScale = new Vector3(
                    transform.localScale.x * cell.transform.localScale.x,
                    transform.localScale.y * cell.transform.localScale.y,
                    cell.transform.localScale.z);
                initialPosition += incrementX;
            }
            initialPosition = new Vector3(initialPosX, initialPosition.y, initialPosition.z);
            initialPosition -= decrementY;
        }
    }

    public void GenerateBombs(int? startingCellID = null)
    {
        random = new Random(Seed);
        var position = new Vector3();

        //Fills up the list
        for (int i = 0; i < numberOfColumns; i++)
        {
            for (int j = 0; j < numberOfRows; j++)
            {
                allCellIds.Add(GenerateId(i, j));
            }
        }

        //Shuffles the list a few times so it's more random
        for (int i = 0; i < 10; i++)
        {
            Shuffle(allCellIds);
        }
        
        var nextBombIndex = 0;

        //Generates an amount of bombs defined by bombsAmount
        while (cellIdsWithBombs.Count < numberOfBombs)
        {
            //Adds the bomb's ids to our list of bombs so we can know where they are 
            if (allCellIds[nextBombIndex] == startingCellID)
            {
                nextBombIndex++;
                continue;
            }
            cellIdsWithBombs.Add(allCellIds[nextBombIndex]);

            //Generates our bomb position from its id
            position = CalculatePosition(allCellIds[nextBombIndex]);
            position.z = 0.95f;

            //Creates the bomb
            var bomb = Instantiate(bombPrefab.gameObject, position, bombPrefab.transform.rotation);
            bomb.transform.localScale = new Vector3(
                transform.localScale.x * bomb.transform.localScale.x,
                bomb.transform.localScale.y,
                transform.localScale.y * bomb.transform.localScale.z);
            nextBombIndex++;
        }

        AreBombsGenerated = true;
    }

    /// <summary>
    /// Converts ids into coordinates then converts coordinates into position.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Vector3 CalculatePosition(int id)
    {
        var position = new Vector3(BoardCenterPosition.x - BoardHalf, BoardCenterPosition.y + BoardHalf);
        float x = id / numberOfColumns;
        float y = id % numberOfRows;
        x *= CellSize;
        y *= CellSize;
        position.x += x;
        position.y -= y;

        return position;
    }

    /// <summary>
    /// Randomize lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public void Shuffle<T>(List<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            var rng = random.Next(0, numberOfColumns);
            var rng2 = random.Next(0, numberOfColumns);
            n--;
            var k = rng;
            var z = rng2;
            T value = list[k];
            list[k] = list[z];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Comproves if there are bombs in a designated position.
    /// </summary>
    /// <param name="posicion"></param>
    /// <returns></returns>
    public bool BombExists(Vector3 posicion)
    {
        var generatedId = GenerateId(posicion);

        if (cellIdsWithBombs.Contains(generatedId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RevealNeighbourCells(Cell cell)
    {
        foreach (var neighbourCellPosition in cell.CalculateEightNeighbourCellPositions())
        {
            var idNeighbourCell = GenerateId(neighbourCellPosition);
            cellById[idNeighbourCell].DisplayBombsNear();
        }
    }

    /// <summary>
    /// Generates an id from x and y coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int GenerateId(int columnIndex, int rowIndex)
    {
        var generatedId = columnIndex * numberOfColumns + rowIndex;
        return generatedId;
    }

    /// <summary>
    /// Generates an id from a Vector3 Position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public int GenerateId(Vector3 position)
    {
        position.x += (BoardHalf - BoardCenterPosition.x);
        position.y -= (BoardHalf + BoardCenterPosition.y);
        position.y = Math.Abs(position.y);
        position.x = Math.Abs(position.x);
        position.x /= CellSize;
        position.y /= CellSize;
        var generatedId = (int)(position.x * numberOfColumns + position.y);

        return generatedId;
    }
}