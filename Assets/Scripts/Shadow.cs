using System.Collections;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private GameObject shadowTilePrefab;
    [SerializeField] private Transform[] board;

    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    private const float FallTime = 1f;
    private float _deltaFallTime;

    // private readonly Transform[] board = new Transform[(RightLimit - LeftLimit) * (TopLimit - BottomLimit)]; // use 1 dimension array to optimize speed
    private GameObject _heldBlock;
    private bool _holdInTurn;

    private Transform _holdArea;
    private Transform _spawnPoint;
    private Spawner _spawner;
    private Block _currentControlBlock;
    
    [Tooltip("Offset center to modify when rendering")]
    

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Move)
        {
            // Move();
            // Smash();
            // HoldAndFall();
            // Hold();
        }
    }

    public void UpsertCurrentClone(ref Block onBoardBlock)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("done");
        foreach (Transform tile in _currentControlBlock.gameObject.transform)
        {
            Instantiate(shadowTilePrefab, tile.position, Quaternion.identity, transform);
        }
    }
    
    // move to des and render center pos
    public void MoveTo(Vector3 des)
    {
        transform.position = des;
        transform.position += des - centerPoint.position;
    }

    private void Move()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;
            if(!ValidMovement()) transform.position += Vector3.right;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            if(!ValidMovement()) transform.position += Vector3.left;
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), -90);
            if(!ValidMovement()) 
                transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), 90);

        }  
    }

    private void Smash()
    {
        StartCoroutine(SmashCoroutine());
        // AddToBoard();
        // if(IsFullCols())
        // {
        //     enabled = false;
        //     GameManager.Instance.GameOver();
        //     return;
        // }
        // enabled = false;
        // _spawner.Spawn();
        // _holdInTurn = false; // refactor
        GameManager.Instance.currentState = GameState.Move;
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        while(ValidMovement())
        {
            transform.position += Vector3.down;
        }
        transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.currentState = GameState.Move;
    }
    
    private IEnumerator HoldCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        enabled = false;
        transform.rotation = Quaternion.identity;
        MoveTo(_holdArea.position);

        if(!_heldBlock)
        {
            _heldBlock = transform.gameObject;
            _spawner.Spawn();
        } else {
            _heldBlock.TryGetComponent(out Block tempBlock);
            {
                tempBlock.enabled = true;
            }
            _heldBlock.transform.position = _spawnPoint.position;
            _heldBlock = transform.gameObject;
        }
        _holdInTurn = true;
        yield return null;
        GameManager.Instance.currentState = GameState.Move;
    }

    private void AddToBoard()
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        foreach (Transform child in transform)
        {
            if(child.gameObject.CompareTag("CenterPoint")) 
                continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            board[GetIndexOnBoardTiles(xIndex, yIndex)] = child;
            if (minY > yIndex) minY = yIndex;
            if (maxY < yIndex) maxY = yIndex;
        }
        for(var line = maxY; line >= minY; line--)
        {
            if(IsFullRow(line))
            {
                DeleteFullRow(line);
                RowDown(line);
            }
        }
    }

    private bool IsFullCols()
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.CompareTag("CenterPoint"))
                continue;
            int yIndex = (int)child.position.y;

            if(yIndex > 20)
                return true;
        }
        return false;
    }

    private bool IsFullRow(int y)
    {
        for (int column = LeftLimit; column < RightLimit - LeftLimit; column++)
        {
            if (!board[GetIndexOnBoardTiles(column, y)])
                return false;
        }
        return true;
    }

    private void DeleteFullRow(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            Destroy(board[GetIndexOnBoardTiles(x, y)].gameObject);
            board[GetIndexOnBoardTiles(x, y)] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = LeftLimit; x < RightLimit - LeftLimit; x++)
            {
                if(board[GetIndexOnBoardTiles(x, y)])
                {
                    board[GetIndexOnBoardTiles(x, y - 1)] = board[GetIndexOnBoardTiles(x, y)];
                    board[GetIndexOnBoardTiles(x, y)] = null;
                    board[GetIndexOnBoardTiles(x, y - 1)].position += Vector3.down;
                }
            }
        }
    }

    public bool ValidMovement()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("CenterPoint"))
                continue;
            if(child.position.x < LeftLimit || child.position.x > RightLimit || child.position.y <= BottomLimit)
            {
                return false;
            }

            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            if(board[GetIndexOnBoardTiles(xIndex, yIndex)]) 
                return false;
        }
        return true;
    }

    private int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (RightLimit - LeftLimit) + column;
    }
}
