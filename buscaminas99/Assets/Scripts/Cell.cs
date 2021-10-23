using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    private GameObject cell;
    public GameObject flag;

    // Start is called before the first frame update
    void Start()
    {
        //cell = GameObject.Find("Cell");
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnRightClick();
        }
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
        Debug.Log("Se ha de borrado");
    }
    void OnRightClick()
    {

        Debug.Log("right clicked");
        Instantiate<GameObject>(flag, transform.position, transform.rotation);
    }
}
