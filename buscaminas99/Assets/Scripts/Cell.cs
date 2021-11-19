using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject flag;
    private GameManager gameManager;
    public TextMesh number;
    private int id;
    private int num = 0;
    private Vector3[] variations = new Vector3[8];

    public int Id => id;

    // Start is called before the first frame update
    void Start()
    {
        FillVariations();
        gameManager = FindObjectOfType<GameManager>();
        id = gameManager.GenerateId(gameObject.transform.position);
        gameManager.RegisterCell(this);
        DisplayBombsNear(gameObject.transform.position);
    }

    /// <summary>
    /// Destroys the clicked cell
    /// </summary>
    private void OnMouseDown()
    {
        if (gameManager.IsPlayerAlive)
        {
            Destroy(gameObject);
            if (gameManager.BombExists(gameObject.transform.position)) 
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                gameManager.IsPlayerAlive = false;
            }
            else
            {
                Debug.Log("It's aliiiiiiiive");
            }
        }
    }

    /// <summary>
    /// Puts or removes flags when right click
    /// </summary>
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            if (gameManager.IsPlayerAlive)
            {
                Instantiate(flag, gameObject.transform.position, transform.rotation);
                //controlar que solo se pueda crear una flag haciendo que si le vuelve a dar la elimine en vez de crear otra
            }
        }
    }

    /// <summary>
    /// Fills the variations of position needed by DisplayBombsNear
    /// </summary>
    private void FillVariations()
    {
        variations[0] = new Vector3(-5, 0);
        variations[1] = new Vector3(-5, 5);
        variations[2] = new Vector3(-5, -5);
        variations[3] = new Vector3(0, -5);
        variations[4] = new Vector3(0, 5);
        variations[5] = new Vector3(5, -5);
        variations[6] = new Vector3(5, 0);
        variations[7] = new Vector3(5, 5);
    }

    /// <summary>
    /// Displays a number that represents how many bombs are nearby
    /// </summary>
    /// <param name="position"></param>
    private void DisplayBombsNear(Vector3 position)
    {
        if (gameManager.BombExists(position))
        {
            return;
        }
        for (int i = 0; i < 8; i++) 
        {
            var auxX = position.x + variations[i].x;
            var auxY = position.y + variations[i].y;
            if ((auxX >= -25 && auxX <= 25) && (auxY >= -25 && auxY <= 25) && gameManager.BombExists(position + variations[i])) 
            {
                num++;
            }
        }
        number.text = num.ToString();
        number.gameObject.SetActive(true);
    }

    public void ModifyById(int sentId)
    {
        if (id == sentId)
        {
            //cositas
            /*
            1.Crear una funcion en Cell para mostrar el numero
            2.Calcular ids de cells de alrededor
            3.Para cada id, pedir al gameManager la Cell correspondiente y llamar a la función que hemos creado en 1
            */
        }
    }

    
}
