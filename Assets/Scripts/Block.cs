using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private TetrominoType type;
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material tilePrefab;
    [SerializeField] private Shadow shadow;
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    
    
    private const float FallTime = 1f;
    private float _deltaFallTime;

    // private static readonly Transform[] Board = new Transform[(RightLimit - LeftLimit) * (TopLimit - BottomLimit)]; // use 1 dimension array to optimize speed
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    private Transform _holdArea;
    private Transform _spawnPoint;
    private Spawner _spawner;

    private Board board { get; set; }
    private float stepTime;
    private float stepDelay = 1f;
    private float moveDelay = 0.1f;
    private float lockDelay = 0.5f;
    private float moveTime;
    private float lockTime;
    private Vector2Int[] childs;
    
    [Tooltip("Offset center to modify when rendering")]

    private void Start()
    {
        _holdArea = GameObject.Find("/HoldArea/Hold").transform;
        _spawnPoint = GameObject.Find("/Spawner").transform;
        _spawner = FindObjectOfType<Spawner>();
        _deltaFallTime = FallTime;
    }
    
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        // this.position = position;
        transform.position = position;

        // rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (childs == null) {
            childs = new Vector2Int[BlockData.Childs.Count];
        }

        for (int i = 0; i < childs.Length; i++) {
            childs[i] = BlockData.Childs[type][i];
        }
    }

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Move)
        {
            HandleMoveInput();
            HoldAndFall();
            // Hold();
        }
    }

    // move to des and render center pos
    public void MoveTo(Vector3 des)
    {
        transform.position = des;
        transform.position += des - centerPoint.position;
    }

    private void HandleMoveInput()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (Move(Vector2Int.left))
            {
                stepTime = Time.time + stepTime;
            }
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (Move(Vector2Int.right))
            {
                stepTime = Time.time + stepTime;
            }
        }
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (Move(Vector2Int.right))
            {
                stepTime = Time.time + stepTime;
            }
        }
        
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down))
            {
                stepTime = Time.time + stepTime;
            }
        }
        
    }
    
    private bool Move(Vector2Int translation)
    {
        
        // if(Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     transform.position += Vector3.left;
        //     if(!ValidMovement()) transform.position += Vector3.right;
        // } else if (Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     transform.position += Vector3.right;
        //     if(!ValidMovement()) transform.position += Vector3.left;
        // }
        //
        // if(Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), -90);
        //     if(!ValidMovement()) 
        //         transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), 90);
        //
        // }  
        return false;
    }
    
    private bool Rotate(int rotateAngle)
    {
        var simulateBlock = this;
        simulateBlock.transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), rotateAngle);
        if (board.ValidMovement(simulateBlock))
        {
            transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), rotateAngle);
            return true;
        }
            
        return false;
    }

    private void HoldAndFall()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SmashCoroutine());
            board.AddToBoard(this);
            if(board.IsFullCols())
            {
                enabled = false;
                GameManager.Instance.GameOver();
                return;
            }
            gameObject.tag = "Untagged";
            enabled = false;
            _spawner.Spawn();
            _holdInTurn = false; // refactor
            GameManager.Instance.currentState = GameState.Move;
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
                    _deltaFallTime -= 20 * Time.deltaTime;
                } else {
                    _deltaFallTime -= Time.deltaTime;
                }
            } else {
                transform.position += Vector3.down;
                // if(!ValidMovement()) 
                // {
                //     transform.position += Vector3.up;
                //     AddToBoard();
                //     if(IsFullCols()){
                //         gameObject.tag = "Untagged";
                //         enabled = false;
                //         GameManager.Instance.GameOver();
                //         return;
                //     }
                //     gameObject.tag = "Untagged";
                //     enabled = false;
                //     _spawner.Spawn();
                //     _holdInTurn = false; // refactor
                // }
                _deltaFallTime = FallTime;
            }   
        } 
        
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        while(Move(Vector2Int.down))
        {
            // transform.position += Vector3.down;
        }
        // transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.currentState = GameState.Move;
    }
    
    private IEnumerator HoldCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        gameObject.tag = "Untagged";
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
                tempBlock.tag = "OnBoardBlock";
                tempBlock.enabled = true;
            }
            _heldBlock.transform.position = _spawnPoint.position;
            _heldBlock = transform.gameObject;
        }
        _holdInTurn = true;
        yield return null;
        GameManager.Instance.currentState = GameState.Move;
    }
}
