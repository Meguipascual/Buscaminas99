using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class GameManager : MonoBehaviour
{

    public Cell cellPrefab;
    public Bomb bombPrefab;
    [SerializeField] private readonly int bombsNumber = 18;//Amount of bombs to create
    private int numberOfColumns = 11;//Table columns
    private int numberOfRows = 11;//Table rows
    private bool isPlayerAlive = true;
    List<int> bombs = new List<int>();//Ids' list that have a bomb 
    List<int> allIds = new List<int>();//List of all ids 
    Dictionary<int, Cell> cellById = new Dictionary<int, Cell>();

    public bool AreBombsGenerated { get; set; }
    public bool IsPlayerAlive
    { 
        get => isPlayerAlive; 
        set => isPlayerAlive = value;
    } 

    // Start is called before the first frame update
    void Start()
    {
        GenerateCells();
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
    public void GenerateBombs(int startingCellID)
    {
        Vector3 pos;

        //Fills up the list
        for (int i = 0; i < numberOfColumns; i++)
        {
            for (int j = 0; j < numberOfRows; j++)
            {
                allIds.Add(GenerateId(i, j));
            }
        }

        //Shuffles the list a few times so it's more random
        for (int i = 0; i < 10; i++)
        {
            Shuffle(allIds);
        }

        var nextBombIndex = 0;

        //Generates an amount of bombs defined by bombsAmount
        while (bombs.Count < bombsNumber)
        {
            if (allIds[nextBombIndex] == startingCellID)
            {
                nextBombIndex++;
                continue;
            }
            bombs.Add(allIds[nextBombIndex]);

            //Generates our bomb position from its id
            pos = CalculatePosition(allIds[nextBombIndex]);
            pos.z = 0.95f;

            //Creates the bomb
            Instantiate(bombPrefab.gameObject, pos, bombPrefab.transform.rotation);
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
        Vector3 position = new Vector3(-25, 25);
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

        if (bombs.Contains(generatedId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    /*public bool GetAlive()
    {
        return alive;
    }
    public void SetAlive(bool live)
    {
        isPlayerAlive = live;
    }
    */






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
}
/*
 * Demasiados espacios

BombsNumber puede ser constante, y columnsnumber tambien

No hacer variables públicas
Quitar update si no se va a usar
Usar var para variables en funciones

cell-> cellPrefab en game manager, lo mismo con bomb. Tipar (poner tipo real en lugar de gameObject)
No need for <GameObject> on instantiate

Positions to constants (generatecells)

//Renombrar columnNumber a numberOfColumns
Crear numberOfRows

Comentarios en inglés

posXInincial -> initialPosX (en GenerateCells)

GeneratePosition -> CalculateCellPosition
posicion -> position en GeneratePosition
id -> cellId en GeneratePosition
Summary con ///

Shuffle a extension method porque es generico

GetAlive y SetAlive a propiedad. Cambiar nombre a isPlayerAlive o algo así

GenerateId -> x e y a rowIndex y columnIndex

BombsNear -> DisplayBombsNear (siempre verbo pirmero para metodos, a menos que sean event handler)
Borrar updates

Estilo consistente, poner siempre espacio antes y despues de = (ver field variations en Cell)

En bombsnear, cambiar el if, y usar constantes
*/