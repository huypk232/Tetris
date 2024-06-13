using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum BlockType {
    I = 0,
    J = 1,
    L = 2,
    O = 3,
    S = 4,
    T = 5,
    Z = 6,
}

public class Block : MonoBehaviour
{
    [SerializeField] private BlockType type;
    [SerializeField] private Transform rotationPoint;
    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    private const float FallTime = 1f;
    private float _deltaFallTime;

    [Header("Share object")]
    private static Transform[,] _board = new Transform[RightLimit,TopLimit]; // share among block instances
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    [SerializeField] private Transform holdArea;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Spawner spawner;
    
    [Tooltip("Offset center to modify when rendering")]
    [SerializeField] private Vector3 centerOffset;

    private void Start()
    {
        holdArea = GameObject.Find("/Level/Hold Area/Hold").transform;
        spawnPoint = GameObject.Find("/Level/Spawner").transform;
        spawner = FindObjectOfType<Spawner>();
        _deltaFallTime = FallTime;
    }

    private void Update()
    {
        if (GameManager.instance.currentState == GameState.Move)
        {
            Move();
            HoldAndFall();
            // Hold();
        }
    }

    // render center to destination des
    public void RenderCenter(Vector3 des)
    {
        transform.position = des;
        transform.position += des - transform.TransformPoint(centerOffset);
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

    private void HoldAndFall()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SmashCoroutine());
            AddToBoard();
            if(IsFullCols())
            {
                enabled = false;
                GameManager.instance.GameOver();
                return;
            }
            enabled = false;
            spawner.Spawn();
            _holdInTurn = false; // refactor
            GameManager.instance.currentState = GameState.Move;
        } else if(Input.GetKeyDown(KeyCode.C)) {
            if(!_holdInTurn)
            {
                StartCoroutine(HoldCoroutine());
            }
        } else {
            
            if(_deltaFallTime > 0.0f)
            {
                if(Input.GetKey(KeyCode.DownArrow))
                {
                    _deltaFallTime -= 10 * Time.deltaTime;
                } else {
                    _deltaFallTime -= Time.deltaTime;
                }
            } else {
                transform.position += Vector3.down;
                if(!ValidMovement()) 
                {
                    transform.position += Vector3.up;
                    AddToBoard();
                    if(IsFullCols()){
                        enabled = false;
                        GameManager.instance.GameOver();
                        return;
                    }
                    enabled = false;
                    spawner.Spawn();
                    _holdInTurn = false; // refactor
                }
                _deltaFallTime = FallTime;
            }   
        } 
        
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.instance.currentState = GameState.Wait;
        while(ValidMovement())
        {
            transform.position += Vector3.down;
        }
        transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.currentState = GameState.Move;
    }

    // todo do not need this, remove
    private IEnumerator HoldCoroutine()
    {
        GameManager.instance.currentState = GameState.Wait;
        enabled = false;
        transform.rotation = Quaternion.identity;
        RenderCenter(holdArea.position);

        if(!_heldBlock)
        {
            _heldBlock = transform.gameObject;
            spawner.Spawn();
        } else {
            _heldBlock.TryGetComponent(out Block tempBlock);
            {
                tempBlock.enabled = true;
            }
            _heldBlock.transform.position = spawnPoint.position;
            _heldBlock = transform.gameObject;
        }
        _holdInTurn = true;
        yield return null;
        GameManager.instance.currentState = GameState.Move;
    }

    private void AddToBoard()
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        foreach (Transform child in transform)
        {
            if(child.name == "Center") continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            _board[xIndex, yIndex] = child;
            if (minY > yIndex) minY = yIndex;
            else if (maxY < yIndex) maxY = yIndex;
        }
        // check lines is full ?, todo refactor
        for(var line = maxY; line >= minY; line--)
        {
            if(IsFullLine(line))
            {
                DeleteFullLine(line);
                RowDown(line);
            }
        }
    }

    private bool IsFullCols()
    {
        foreach(Transform child in transform)
        {
            int yIndex = (int)child.position.y;

            if(yIndex > 20)
                return true;
        }
        return false;
    }

    private void CheckLines()
    {
        for(int i = TopLimit-1; i >= 0; i--)
        {
            if(IsFullLine(i))
            {
                DeleteFullLine(i);
                RowDown(i);
            }
        }
    }

    private static bool IsFullLine(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            if (!_board[x, y])
                return false;
        }
        return true;
    }

    private static void DeleteFullLine(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            Destroy(_board[x, y].gameObject);
            _board[x, y] = null;
        }
    }

    private static void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = 0; x < RightLimit; x++)
            {
                if(_board[x, y])
                {
                    _board[x, y - 1] = _board[x, y];
                    _board[x, y] = null;
                    _board[x, y - 1].position += Vector3.down;
                }
            }
        }
    }

    public bool ValidMovement()
    {
        foreach (Transform child in transform)
        {
            if(child.position.x < LeftLimit || child.position.x > RightLimit || child.position.y <= BottomLimit)
            {
                return false;
            }

            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            if(_board[xIndex, yIndex]) 
                return false;
        }
        return true;
    }
}
