using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject cell;
    public GameObject bomb;
    public int bombsNumber = 1;

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
    public void Shuffle<T>( List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int rng = Random.Range(0,11);
            int rng2 = Random.Range(0, 11);

           

            n--;
            int k = rng;
            int z = rng2;

            T value = list[k];
            list[k] = list[z];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public Vector3 GeneratePosition (int id)
    {
        ////////
        ///

        

        ///
        ////////

        Vector3 posicion=new Vector3(-25,25);
        int y = id / 11;
        int x = id % 11;

        Debug.Log("y es igual a : " + y);
        Debug.Log("x es igual a : " + x);


        x *= 5;
        y *= 5;
        posicion.x += x;
        posicion.y -= y;

        return posicion;
    }

    void GenerateCells()
    {
        Vector3 pos = new Vector3(-25, 25, -0.4f);
        Vector3 incrementoX = new Vector3(5,0,0);
        Vector3 decrementoY = new Vector3(0,5,0);
        float posXInincial = -25f;

        for (int i =0; i < 11; i++)
        {
            for(int j = 0; j < 11; j++)
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

        /////////
        ///
        List<int> ids = new List<int>();

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                ids.Add(i * 11 + j);
            }
        }


        Shuffle(ids);
        Shuffle(ids);
        Shuffle(ids);

        for (int j = 0; j < bombsAmount; j++)
        {
            //Debug.Log(ids[j]);
            
            pos = GeneratePosition(ids[j]);
            pos.z = 0.95f;
            Instantiate<GameObject>(bomb, pos, bomb.transform.rotation);
        }
        ///
        /////////

        //for (int i =0; i < bombsAmount; i++)
        //{
            
            //int randomX = Random.Range(0,11);
            //int randomY = Random.Range(0, 11);

            //GameObject [] bombas= GameObject.FindGameObjectsWithTag("Bomb");
            //Debug.Log(bombas.Length);
            

            
        
            
            
            

        //}
        
    }
}
