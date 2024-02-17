using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject _flag;
    private GameManager _gameManager;
    [SerializeField] private TextMesh _number;
    [SerializeField] private MeshRenderer _renderer;
    private int _id;
    private int _numberSurroundingBombs;
    private bool _isCellExplored;
    private Vector3[] _variations = new Vector3[8];

    public int Id => _id;

    // Start is called before the first frame update
    void Start()
    {
        FillVariations();
        _gameManager = FindObjectOfType<GameManager>();
        _id = _gameManager.GenerateId(gameObject.transform.position);
        _gameManager.RegisterCell(this);
    }

    /// <summary>
    /// Destroys the clicked cell
    /// </summary>
    private void OnMouseDown()
    {
        if (_isCellExplored) { return; }

        if (!_gameManager.AreBombsGenerated)
        {
            _gameManager.GenerateBombs(_id);
        }

        if (_gameManager.IsPlayerAlive)
        {
            DisplayBombsNear(gameObject.transform.position);
            
            if (_gameManager.BombExists(gameObject.transform.position)) 
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                _gameManager.IsPlayerAlive = false;
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
            if (_gameManager.IsPlayerAlive)
            {
                Instantiate(_flag, gameObject.transform.position, transform.rotation);
                //controlar que solo se pueda crear una flag haciendo que si le vuelve a dar la elimine en vez de crear otra
            }
        }
    }

    /// <summary>
    /// Fills the variations of position needed by DisplayBombsNear
    /// </summary>
    private void FillVariations()
    {
        _variations[0] = new Vector3(-5, 0);
        _variations[1] = new Vector3(-5, 5);
        _variations[2] = new Vector3(-5, -5);
        _variations[3] = new Vector3(0, -5);
        _variations[4] = new Vector3(0, 5);
        _variations[5] = new Vector3(5, -5);
        _variations[6] = new Vector3(5, 0);
        _variations[7] = new Vector3(5, 5);
    }

    /// <summary>
    /// Displays a number that represents how many bombs are nearby
    /// </summary>
    /// <param name="position"></param>
    private void DisplayBombsNear(Vector3 position)
    {
        if (_isCellExplored)
        {
            return;
        }

        _renderer.enabled = false;
        _isCellExplored = true;

        if (_gameManager.BombExists(position))
        {
            return;
        }
        
        for (int i = 0; i < 8; i++) 
        {
            var auxX = position.x + _variations[i].x;
            var auxY = position.y + _variations[i].y;
            if ((auxX >= -25 && auxX <= 25) && (auxY >= -25 && auxY <= 25) && _gameManager.BombExists(position + _variations[i])) 
            {
                _numberSurroundingBombs++;
            }
        }

        if (_numberSurroundingBombs > 0)
        {
            _number.text = _numberSurroundingBombs.ToString();
            _number.gameObject.SetActive(true);
        }

        if(_numberSurroundingBombs == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                var auxX = position.x + _variations[i].x;
                var auxY = position.y + _variations[i].y;
                var newCellId = _gameManager.GenerateId(new Vector3(auxX, auxY, 0));
                if ((auxX >= -25 && auxX <= 25) && (auxY >= -25 && auxY <= 25))
                {
                    _gameManager.GetCell(newCellId).DisplayBombsNear(new Vector3(auxX, auxY, 0));
                }
            }
        }
    }

    public void ModifyById(int sentId)
    {
        if (_id == sentId)
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
