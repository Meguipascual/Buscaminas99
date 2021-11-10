using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    private GameObject cell;
    public GameObject flag;
    private GameManager gameManager;
    public TextMesh number;
    int numero = 0;
    bool alive ;

    // Start is called before the first frame update
    void Start()
    {
        
        gameManager = FindObjectOfType<GameManager>();
        //number = GameObject.Find("Number").GetComponent<TextMesh>();
        //number = GameObject.Find("Number").GetComponentInChildren<TextMesh>();
        
        
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        alive = gameManager.GetAlive();
        if (alive)
        {
            Destroy(gameObject);
            if (gameManager.BombExists(gameObject.transform.position))
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                gameManager.SetAlive(false);
            }
            else
            {
                Debug.Log("U are aive");
            }
        }
        
        
    }
    private void OnMouseOver()
    {
        

        if (Input.GetMouseButtonDown(1)) 
        {
            alive = gameManager.GetAlive();

            if (alive)
            {
                Instantiate<GameObject>(flag, gameObject.transform.position, transform.rotation);
                number.text = "2";
            }
            
        }
    }

    
}
