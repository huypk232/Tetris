using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private BlockType type;
    [SerializeField] private Transform rotationPoint;
    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    private const float FallTime = 1f;
    private float _deltaFallTime;

    // [FormerlySerializedAs("_board")]
    [Header("Share object")]
    // private static Transform[][] board = new Transform[RightLimit - LeftLimit][5]; // share among block instances
    // public GameObject boardGo; // share among block instances
    private static GameObject _heldBlock;
    private static bool _holdInTurn;
    private Transform[][] board;

    private Transform _holdArea;
    private Transform _spawnPoint;
    private Spawner _spawner;
    
    [Tooltip("Offset center to modify when rendering")]
    [SerializeField] private Vector3 centerOffset;
    
    public Transform[][] _tiles = new Transform[RightLimit - LeftLimit][]; // share among block instances
    
    private void Awake()
    {
        for (int column = 0; column < RightLimit - LeftLimit; column++)
        {
            _tiles[column] = new Transform[TopLimit - BottomLimit + 2];
        }
    }
    
    private void Update()
    {
        // if (GameManager.instance.currentState == GameState.Move)
        // {
        //     Move();
        //     HoldAndFall();
        //     // Hold();
        // }
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

    // private void HoldAndFall()
    // {
    //     if(Input.GetKeyDown(KeyCode.Space))
    //     {
    //         StartCoroutine(SmashCoroutine());
    //         AddToBoard();
    //         if(IsFullCols())
    //         {
    //             enabled = false;
    //             GameManager.instance.GameOver();
    //             return;
    //         }
    //         enabled = false;
    //         _spawner.Spawn();
    //         _holdInTurn = false; // refactor
    //         GameManager.instance.currentState = GameState.Move;
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
    //                     GameManager.instance.GameOver();
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
        GameManager.instance.currentState = GameState.Move;
    }

    public void AddToBoard(GameObject block)
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        foreach (Transform child in block.transform)
        {
            // if(child.name == "Center") continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            _tiles[xIndex][yIndex] = child;
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

            if(yIndex > 20) // todo make constant
                return true;
        }
        return false;
    }
    
    private bool IsFullLine(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            if (!_tiles[x][y])
                return false;
        }
        return true;
    }

    private void DeleteFullLine(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            Destroy(_tiles[x][y].gameObject);
            _tiles[x][y] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = 0; x < RightLimit; x++)
            {
                Debug.Log(_tiles[x][y]);
                if(_tiles[x][y])
                {
                    _tiles[x][y - 1] = _tiles[x][y];
                    _tiles[x][y] = null;
                    _tiles[x][y - 1].position += Vector3.down;
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
            if(_tiles[xIndex][yIndex]) 
                return false;
        }
        return true;
    }
}
