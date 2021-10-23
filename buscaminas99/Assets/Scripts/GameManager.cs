using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject cell;
    public GameObject bomb;

    // Start is called before the first frame update
    void Start()
    {
        GenerateCells();
        GenerateBombs();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GeneratePosition (int y, int x)
    {
        Vector3 posicion=new Vector3(-25,25);

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
    void GenerateBombs()
    {
        Vector3 pos;
        bool exist = false;


        for(int i =0; i < 120; i++)
        {
            
            int randomX = Random.Range(0,11);
            int randomY = Random.Range(0, 11);
            GameObject [] bombas= GameObject.FindGameObjectsWithTag("Bomb");
            Debug.Log(bombas.Length);
            pos = GeneratePosition(randomY, randomX);
            pos.z = 0.95f;
            


            for (int j =0;j < bombas.Length; j++)
            {
                if ((bombas[j].transform.position.y.Equals(pos.y)) && (bombas[j].transform.position.x.Equals(pos.x)))
                {
                    exist = true;

                    Debug.Log("existe");
                }
                
            }
            

            if (!exist)
            {
                Instantiate<GameObject>(bomb, pos, bomb.transform.rotation);
            }
            exist = false;

        }
        
    }
}
