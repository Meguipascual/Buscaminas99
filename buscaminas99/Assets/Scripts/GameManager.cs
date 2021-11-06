using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class GameManager : MonoBehaviour
{

    public GameObject cell;
    public GameObject bomb;
    public int bombsNumber = 18;//cantidad de bombas a crear
    private int columnNumber = 11;//tama�o del tablero
    List<int> bombs = new List<int>();//la lista de ids de bombas en tablero

    // Start is called before the first frame update
    void Start()
    {
        GenerateCells();
        GenerateBombs(bombsNumber);
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /*Genera las celdas en todo el tablero, podria existir la variable 
        numeroFilas en caso de gestionar tableros rectangulares*/
    void GenerateCells()
    {
        

        Vector3 pos = new Vector3(-25, 25, -0.4f);
        Vector3 incrementoX = new Vector3(5,0,0);
        Vector3 decrementoY = new Vector3(0,5,0);
        float posXInincial = -25f;

        for (int i =0; i < columnNumber; i++)
        {
            for(int j = 0; j < columnNumber; j++)
            {
                Instantiate<GameObject>(cell, pos, cell.transform.rotation);

                pos += incrementoX;
            }
            pos = new Vector3(posXInincial, pos.y,pos.z);
            pos -= decrementoY;
        }
        
    }
    void GenerateBombs(int bombsAmount)
    {
        Vector3 pos;

        //lista de todas las ids de celda
        List<int> ids = new List<int>();


        //Rellenamos la lista
        for (int i = 0; i < columnNumber; i++)
        {
            for (int j = 0; j < columnNumber; j++)
            {
                ids.Add(i * columnNumber + j);
            }
        }


        //Mezclamos la lista varias veces para conseguir mayor aleatoriedad
        for(int i = 0; i < 10; i++)
        {
            Shuffle(ids);
        }

        //generamos una cantidad de bombas definida por bombsAmount
        for (int j = 0; j < bombsAmount; j++)
        {
            Debug.Log(ids[j]);

            //a�adimos las bombas que creamos a una lista de bombas para poder comprobar donde estan todas nuestras bombas
            bombs.Add(ids[j]);

            //Generamos la posicion de nuestra bomba a partir de su id
            pos = GeneratePosition(ids[j]);
            pos.z = 0.95f;

            //creamos las bombas
            Instantiate<GameObject>(bomb, pos, bomb.transform.rotation);
        }
        
    }

    /*las posiciones van de 5 en 5, este metodo se ocupa de convertir la id en coordenadas
    multiplicarlas por 5 y sumarlas o restarlas a la posicion de la coordenada 0,0*/
    public Vector3 GeneratePosition(int id)
    {
        

        Vector3 posicion = new Vector3(-25, 25);
        int x = id / columnNumber;
        int y = id % columnNumber;

        Debug.Log("y es igual a : " + y);
        Debug.Log("x es igual a : " + x);


        x *= 5;
        y *= 5;
        posicion.x += x;
        posicion.y -= y;

        return posicion;
    }

    //Randomiza la lista, un poco
    public void Shuffle<T>(List<T> list)
    {
        

        int n = list.Count;
        while (n > 1)
        {
            int rng = UnityEngine.Random.Range(0, columnNumber);
            int rng2 = UnityEngine.Random.Range(0, columnNumber);



            n--;
            int k = rng;
            int z = rng2;

            T value = list[k];
            list[k] = list[z];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //compueba si existen bombas en una posicion indicada
    public bool BombExists(Vector3 posicion)
    {
        
        posicion.x += 25;
        posicion.y -= 25;

        posicion.y = Math.Abs(posicion.y);
        posicion.x = Math.Abs(posicion.x);

        posicion.x /= 5;
        posicion.y /= 5;

       
        int id =(int)( posicion.x * columnNumber + posicion.y);

        if (bombs.Contains(id))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
