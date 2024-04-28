using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject _flagPrefab;
    [SerializeField] private TextMesh _number;
    [SerializeField] private MeshRenderer _renderer;
    
    private GameManager _gameManager;
    private BoardManager _boardManager;
    private ClientManager _clientManager;

    private GameObject _flag;

    private int _id;
    private bool _isCellExplored;

    private Vector3[] eightVariations = new Vector3[8];
    private Vector3[] fourVariations = new Vector3[4];

    public int Id => _id;
    public bool IsExplored => _isCellExplored;

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
        Debug.Log($"Mouse down on cell {_id}");
        if (_isCellExplored || _boardManager.IsRivalBoard || !_gameManager.IsGameActive) { return; }

        if (!_boardManager.AreBombsGenerated)
        {
            _boardManager.GenerateBombs(_id);
        }
        
        if (_gameManager.IsPlayerAlive)
        {
            var discoverCellIds = UseCell();
            _clientManager.SendCellIdMessage(_id, discoverCellIds);
            
        }
    }

    public List<int> UseCell()
    {
        List<int> revealedCellIds = new List<int>();

        if (_boardManager.BombExists(gameObject.transform.position))
        {
            HideCell();
            if (!_boardManager.IsRivalBoard)
            {
                Debug.Log("GameOver, te has murido muy fuerte");
                _gameManager.KillPlayer();
            }
        }
        else
        {
            DisplayBombsNear();
            Debug.Log("It's aliiiiiiiive");
            revealedCellIds = _boardManager.RevealNeighbourCells(this);
        }
        return revealedCellIds;
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
                _flag = Instantiate(_flagPrefab, gameObject.transform.position, transform.rotation);
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
    public List<int> DisplayBombsNear()
    {
        List<int> discoverCellIds = new List<int>();
        if (_isCellExplored)
        {
            return discoverCellIds;
        }
        
        Debug.Log($"Exploring cell {_id}");

        var position = gameObject.transform.position;
        var num = 0;
        if (_boardManager.BombExists(position))
        {
            Debug.Log($"Bomb exists in cell {_id}");
            return discoverCellIds;
        }
        
        _isCellExplored = true;

        foreach (var neighbourPosition in CalculateEightNeighbourCellPositions())
        {
            if (_boardManager.BombExists(neighbourPosition)) {
                num++;
            }
        }

        if (num == 0)
        {
            Debug.Log($"No bombs surrounding cell {_id}, will explore neighbours");
            discoverCellIds = _boardManager.RevealNeighbourCells(this);
        }
        else
        {
            Debug.Log($"Bombs found in cell {_id}, writing number");
            _number.text = num.ToString();
            _number.gameObject.SetActive(true);
        }
        Debug.Log($"Hiding cell {_id}");
        HideCell();
        discoverCellIds.Add(Id);
        return discoverCellIds;
    }

    private void HideCell()
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

    public void Destroy() {
        Destroy(_number.gameObject);
        Destroy(_renderer.gameObject);
        if (_flag != null) {
            Destroy(_flag.gameObject);
        }
        Destroy(gameObject);
    }
}
