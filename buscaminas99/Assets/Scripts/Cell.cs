using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    private GameObject cell;
    public GameObject flag;
    private GameManager gameManager;
    public GameObject number;

    // Start is called before the first frame update
    void Start()
    {
        
        gameManager = FindObjectOfType<GameManager>();
        number = GetComponent<GameObject>();
       
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
        if (gameManager.BombExists(gameObject.transform.position))
        {
            Debug.Log("GameOver, te has murido muy fuerte");
        }
        else
        {
            Debug.Log("U are aive");
        }
        
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Instantiate<GameObject>(flag, gameObject.transform.position, transform.rotation);
        }
    }

    
}
