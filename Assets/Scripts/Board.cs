using System.Collections;
using UnityEngine;



public class Board : MonoBehaviour
{
    [SerializeField] private BlockType type;
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material tilePrefab;
    
    public const int LeftLimit = 0;
    public const int RightLimit = 10;
    public const int BottomLimit = 0;
    public const int TopLimit = 23;
    private const float FallTime = 1f;
    private float _deltaFallTime;

    public readonly Transform[] _tiles = new Transform[(RightLimit - LeftLimit) * (TopLimit - BottomLimit)]; // use 1 dimension array to optimize speed
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    private Transform _holdArea;
    private Transform _spawnPoint;
    private Spawner _spawner;
    
    [Tooltip("Offset center to modify when rendering")]

    private void Start()
    {
        _holdArea = GameObject.Find("/HoldArea/Hold").transform;
        _spawnPoint = GameObject.Find("/Spawner").transform;
        _spawner = FindObjectOfType<Spawner>();
        _deltaFallTime = FallTime;
    }

    // private void Update()
    // {
    //     if (GameManager.Instance.currentState == GameState.Move)
    //     {
    //         Move();
    //         HoldAndFall();
    //         // Hold();
    //     }
    // }

    // private void Move()
    // {
    //     if(Input.GetKeyDown(KeyCode.LeftArrow))
    //     {
    //         transform.position += Vector3.left;
    //         if(!ValidMovement()) transform.position += Vector3.right;
    //     } else if (Input.GetKeyDown(KeyCode.RightArrow))
    //     {
    //         transform.position += Vector3.right;
    //         if(!ValidMovement()) transform.position += Vector3.left;
    //     }
    //
    //     if(Input.GetKeyDown(KeyCode.UpArrow))
    //     {
    //         transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), -90);
    //         if(!ValidMovement()) 
    //             transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), 90);
    //
    //     }  
    // }

    // private void HoldAndFall()
    // {
    //     if(Input.GetKeyDown(KeyCode.Space))
    //     {
    //         StartCoroutine(SmashCoroutine());
    //         AddToBoard();
    //         if(IsFullCols())
    //         {
    //             enabled = false;
    //             GameManager.Instance.GameOver();
    //             return;
    //         }
    //         enabled = false;
    //         _spawner.Spawn();
    //         _holdInTurn = false; // refactor
    //         GameManager.Instance.currentState = GameState.Move;
    //     } else if(Input.GetKeyDown(KeyCode.C)) {
    //         if(!_holdInTurn)
    //         {
    //             StartCoroutine(HoldCoroutine());
    //         }
    //     } else {
    //         
    //         if(_deltaFallTime > 0.0f)
    //         {
    //             if(Input.GetKey(KeyCode.DownArrow))
    //             {
    //                 _deltaFallTime -= 10 * Time.deltaTime;
    //             } else {
    //                 _deltaFallTime -= Time.deltaTime;
    //             }
    //         } else {
    //             transform.position += Vector3.down;
    //             if(!ValidMovement()) 
    //             {
    //                 transform.position += Vector3.up;
    //                 AddToBoard();
    //                 if(IsFullCols()){
    //                     enabled = false;
    //                     GameManager.Instance.GameOver();
    //                     return;
    //                 }
    //                 enabled = false;
    //                 _spawner.Spawn();
    //                 _holdInTurn = false; // refactor
    //             }
    //             _deltaFallTime = FallTime;
    //         }   
    //     } 
    //     
    // }

    // private IEnumerator SmashCoroutine()
    // {
    //     GameManager.Instance.currentState = GameState.Wait;
    //     while(ValidMovement())
    //     {
    //         transform.position += Vector3.down;
    //     }
    //     transform.position += Vector3.up;
    //     yield return new WaitForSeconds(0.1f);
    //     GameManager.Instance.currentState = GameState.Move;
    // }
    //
    // private IEnumerator HoldCoroutine()
    // {
    //     GameManager.Instance.currentState = GameState.Wait;
    //     enabled = false;
    //     transform.rotation = Quaternion.identity;
    //     MoveTo(_holdArea.position);
    //
    //     if(!_heldBlock)
    //     {
    //         _heldBlock = transform.gameObject;
    //         _spawner.Spawn();
    //     } else {
    //         _heldBlock.TryGetComponent(out Block tempBlock);
    //         {
    //             tempBlock.enabled = true;
    //         }
    //         _heldBlock.transform.position = _spawnPoint.position;
    //         _heldBlock = transform.gameObject;
    //     }
    //     _holdInTurn = true;
    //     yield return null;
    //     GameManager.Instance.currentState = GameState.Move;
    // }

    public void AddToBoard(Block block)
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        foreach (Transform child in block.gameObject.transform)
        {
            if(child.gameObject.CompareTag("CenterPoint")) 
                continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            _tiles[GetIndexOnBoardTiles(xIndex, yIndex)] = child;
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

    public bool IsFullCols(Block block)
    {
        foreach(Transform child in block.gameObject.transform)
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
            if (!_tiles[GetIndexOnBoardTiles(column, y)])
                return false;
        }
        return true;
    }

    private void DeleteFullRow(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            Destroy(_tiles[GetIndexOnBoardTiles(x, y)].gameObject);
            _tiles[GetIndexOnBoardTiles(x, y)] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = LeftLimit; x < RightLimit - LeftLimit; x++)
            {
                if(_tiles[GetIndexOnBoardTiles(x, y)])
                {
                    _tiles[GetIndexOnBoardTiles(x, y - 1)] = _tiles[GetIndexOnBoardTiles(x, y)];
                    _tiles[GetIndexOnBoardTiles(x, y)] = null;
                    _tiles[GetIndexOnBoardTiles(x, y - 1)].position += Vector3.down;
                }
            }
        }
    }

    public bool ValidMovement(Block block)
    {
        foreach (Transform child in block.gameObject.transform)
        {
            if (child.gameObject.CompareTag("CenterPoint"))
                continue;
            if(child.position.x < LeftLimit || child.position.x > RightLimit || child.position.y <= BottomLimit)
            {
                return false;
            }

            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            if(_tiles[GetIndexOnBoardTiles(xIndex, yIndex)]) 
                return false;
        }
        return true;
    }

    private static int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (RightLimit - LeftLimit) + column;
    }
}
