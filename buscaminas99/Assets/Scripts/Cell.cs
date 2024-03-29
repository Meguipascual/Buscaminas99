using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject _flag;
    [SerializeField] private TextMesh _number;
    [SerializeField] private MeshRenderer _renderer;
    
    private GameManager _gameManager;
    private BoardManager _boardManager;
    private ClientManager _clientManager;

    private int _id;
    private bool _isCellExplored;
    
    private Vector3[] eightVariations = new Vector3[8];
    private Vector3[] fourVariations = new Vector3[4];

    public int Id => _id;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _clientManager = FindObjectOfType<ClientManager>();
        FillVariations();
        _id = _boardManager.GenerateId(gameObject.transform.position);
        _boardManager.RegisterCell(this);
    }

    public void SetBoardManager(BoardManager boardManager)
    {
        this._boardManager = boardManager;
    }

    private void OnMouseDown()
    {
        if (_isCellExplored || _boardManager.IsRivalBoard) { return; }

        if (!_boardManager.AreBombsGenerated)
        {
            _boardManager.GenerateBombs(_id);
        }
        
        if (_gameManager.IsPlayerAlive)
        {
            _clientManager.SendCellIdMessage(_id);
            UseCell();
        }
    }

    public void UseCell()
    {
        if (_boardManager.BombExists(gameObject.transform.position))
        {
            DestroyCell();
            if (!_boardManager.IsRivalBoard)
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                _gameManager.IsPlayerAlive = false;
                _gameManager.gameOutcomeText.gameObject.SetActive(true);
            }
        }
        else
        {
            DisplayBombsNear();
            Debug.Log("It's aliiiiiiiive");
            _boardManager.RevealNeighbourCells(this);
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
        eightVariations[0] = new Vector3(-_boardManager.CellSize, 0);
        eightVariations[1] = new Vector3(-_boardManager.CellSize, _boardManager.CellSize);
        eightVariations[2] = new Vector3(-_boardManager.CellSize, -_boardManager.CellSize);
        eightVariations[3] = new Vector3(0, -_boardManager.CellSize);
        eightVariations[4] = new Vector3(0, _boardManager.CellSize);
        eightVariations[5] = new Vector3(_boardManager.CellSize, -_boardManager.CellSize);
        eightVariations[6] = new Vector3(_boardManager.CellSize, 0);
        eightVariations[7] = new Vector3(_boardManager.CellSize, _boardManager.CellSize);
    }

    /// <summary>
    /// Displays a number that represents how many bombs are nearby
    /// </summary>
    public void DisplayBombsNear()
    {
        if (_isCellExplored)
        {
            return;
        }
        
        _isCellExplored = true;

        var position = gameObject.transform.position;
        var num = 0;
        if (_boardManager.BombExists(position))
        {
            return;
        }

        foreach (var neighbourPosition in CalculateEightNeighbourCellPositions())
        {
            if (_boardManager.BombExists(neighbourPosition)) {
                num++;
            }
        }

        if (num == 0)
        {
            _boardManager.RevealNeighbourCells(this);
        }
        else
        {
            _number.text = num.ToString();
            _number.gameObject.SetActive(true);
        }
        DestroyCell();
    }

    private void DestroyCell()
    {
        _renderer.enabled = false;

        if (!_boardManager.IsRivalBoard)
        {
            _gameManager.TrackCellRevealed(Id);
        }
    }

    public IEnumerable<Vector3> CalculateEightNeighbourCellPositions() {
        var position = gameObject.transform.position;
        for (int i = 0; i < 8; i++)
        {
            var auxX = position.x + eightVariations[i].x;
            var auxY = position.y + eightVariations[i].y;
            if (auxX >= (_boardManager.BoardCenterPosition.x - _boardManager.BoardHalf) - GlobalConstants.FloatPrecision
                && auxX <= (_boardManager.BoardCenterPosition.x + _boardManager.BoardHalf) + GlobalConstants.FloatPrecision
                && auxY >= (_boardManager.BoardCenterPosition.y - _boardManager.BoardHalf) - GlobalConstants.FloatPrecision
                && auxY <= (_boardManager.BoardCenterPosition.y + _boardManager.BoardHalf) + GlobalConstants.FloatPrecision)
            {
                yield return position + eightVariations[i];
            }
        }
    }
}
