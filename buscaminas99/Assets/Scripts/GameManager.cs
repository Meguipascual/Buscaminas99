using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;


public class GameManager : MonoBehaviour
{

    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Bomb bombPrefab;
    public TextMeshProUGUI deadText;
    private readonly int numberOfBombs = 18;//Amount of bombs to create
    private readonly int numberOfColumns = 11;//Table columns
    private readonly int numberOfRows = 11;//Table rows
    List<int> cellIdsWithBombs = new List<int>();//Ids' list that have a bomb 
    List<int> allCellIds = new List<int>();//List of all ids 
    Dictionary<int, Cell> cellById = new Dictionary<int, Cell>();//Dictionary that relates an ID with its Cell
    HashSet<int> revealedCellIds = new HashSet<int>();//HashSet that Stores all the revealed ID cells
    public bool IsPlayerAlive { get; set; } = true; //Simplifies the get/set structure for a boolean to be accesed by other classes

    // Start is called before the first frame update
    void Start()
    {
        GenerateCells();
        GenerateBombs(numberOfBombs);
    }

    public void RegisterCell(Cell cell)
    {
        cellById[cell.Id] = cell;
    }

    public Cell GetCell (int cellId)
    {
        return cellById[cellId];
    }

    //Generates cells all over the table
    void GenerateCells()
    {
        var initialPosition = new Vector3(-25, 25, -0.4f);
        var incrementX = new Vector3(5,0,0);
        var decrementY = new Vector3(0,5,0);
        var initialPosX = -25f;

        for (int i = 0; i < numberOfColumns; i++) 
        {
            for(int j = 0; j < numberOfRows; j++)
            {
                Instantiate(cellPrefab.gameObject, initialPosition, cellPrefab.transform.rotation);
                initialPosition += incrementX;
            }
            initialPosition = new Vector3(initialPosX, initialPosition.y, initialPosition.z);
            initialPosition -= decrementY;
        }
    }

    void GenerateBombs(int bombsAmount)
    {
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

        //Generates an amount of bombs defined by bombsAmount
        for (int j = 0; j < bombsAmount; j++)
        {
            //Adds the bomb's ids to our list of bombs so we can know where they are 
            cellIdsWithBombs.Add(allCellIds[j]);

            //Generates our bomb position from its id
            position = CalculatePosition(allCellIds[j]);
            position.z = 0.95f;

            //Creates the bomb
            Instantiate(bombPrefab.gameObject, position, bombPrefab.transform.rotation);
        }
        
    }

    /// <summary>
    /// Converts ids into coordinates then converts coordinates into position.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Vector3 CalculatePosition(int id)
    {
        var position = new Vector3(-25, 25);
        var x = id / numberOfColumns;
        var y = id % numberOfRows;
        x *= 5;
        y *= 5;
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
            var rng = UnityEngine.Random.Range(0, numberOfColumns);
            var rng2 = UnityEngine.Random.Range(0, numberOfColumns);
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
            if (revealedCellIds.Contains(idNeighbourCell))
            {
                continue;
            }
            revealedCellIds.Add(idNeighbourCell);
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
        var generatedId= columnIndex * numberOfColumns + rowIndex;
        return generatedId;
    }

    /// <summary>
    /// Generates an id from a Vector3 Position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public int GenerateId(Vector3 position)
    {
        position.x += 25;
        position.y -= 25;
        position.y = Math.Abs(position.y);
        position.x = Math.Abs(position.x);
        position.x /= 5;
        position.y /= 5;
        var generatedId = (int)(position.x * numberOfColumns + position.y);
        
        return generatedId;
    }

    public void Reset()
    {
        SceneManager.LoadScene("MineSweeper");
    }
}