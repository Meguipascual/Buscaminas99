using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    private GameObject cell;
    public GameObject flag;
    private GameManager gameManager;
    public TextMesh number;
    private int num = 0;
    private Vector3[] variations=new Vector3 [8];


    // Start is called before the first frame update
    void Start()
    {
        
        variations[0] = new Vector3(-5, 0);
        variations[1] = new Vector3(-5, 5);
        variations[2] = new Vector3(-5, -5);
        variations[3] = new Vector3(0, -5);
        variations[4] = new Vector3(0, 5);
        variations[5] = new Vector3(5, -5);
        variations[6] = new Vector3(5, 0);
        variations[7] = new Vector3(5, 5);

        gameManager = FindObjectOfType<GameManager>();
        BombsNear(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        
        if (gameManager.GetAlive())
        {
            Destroy(gameObject);
            if (gameManager.BombExists(gameObject.transform.position))
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                gameManager.SetAlive(false);
            }
            else
            {
                Debug.Log("It's aliiiiiiiive");
            }
        }
        
        
    }
    private void OnMouseOver()
    {
        

        if (Input.GetMouseButtonDown(1)) 
        {

            if (gameManager.GetAlive())
            {
                Instantiate<GameObject>(flag, gameObject.transform.position, transform.rotation);
                //controlar que solo se pueda crear una flag haciendo que si le vuelve a dar la elimine en vez de crear otra
                number.text = "2";
            }
            
        }
    }

    private void BombsNear(Vector3 position)
    {




        if (!gameManager.BombExists(position))
        {

            for (int i = 0; i < 8; i++)
            {
                float auxX = position.x + variations[i].x;
                float auxY = position.y + variations[i].y;
                if ((auxX >= -25 && auxX <= 25)&& (auxY >= -25 && auxY <= 25))
                {
                    if (gameManager.BombExists(position + variations[i]))
                    {
                        num++;
                    }
                }

                


            }
            number.text = num.ToString();

        }
        else
        {
            number.text = "B";
        }


        
        
        
    }

    
}
