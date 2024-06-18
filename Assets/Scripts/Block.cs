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
    
    
    private float _deltaFallTime;

    // private static readonly Transform[] Board = new Transform[(RightLimit - LeftLimit) * (TopLimit - BottomLimit)]; // use 1 dimension array to optimize speed
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    private Transform _holdArea;
    private Transform _spawnPoint;
    private Spawner _spawner;

    public Board board { get; set; }
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float moveDelay = 0.1f;
    [SerializeField] private float lockDelay = 0.5f;
    
    private float stepTime;
    private float _moveTime;
    private float lockTime;

    private void Start()
    {
        _holdArea = GameObject.Find("/HoldArea/Hold").transform;
        _spawnPoint = GameObject.Find("/Spawner").transform;
        _spawner = FindObjectOfType<Spawner>();
        _deltaFallTime = stepTime;

        stepTime = Time.time;
        _moveTime = Time.time;
        lockTime = Time.time;
    }
    
    // public void Initialize(Board board, Vector3Int position, TetrominoData data)
    // {
    //     this.data = data;
    //     this.board = board;
    //     // this.position = position;
    //     transform.position = position;
    //
    //     // rotationIndex = 0;
    //     stepTime = Time.time + stepDelay;
    //     moveTime = Time.time + moveDelay;
    //     lockTime = 0f;
    //
    //     if (childs == null) {
    //         childs = new Vector2Int[BlockData.Childs.Count];
    //     }
    //
    //     for (int i = 0; i < childs.Length; i++) {
    //         childs[i] = BlockData.Childs[type][i];
    //     }
    // }

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Move)
        {
            if (Time.time > _moveTime)
            {
                HandleMoveInputs();
            }

            Hold();
            Smash();
            Drop();
            // HoldAndFall();
            // Hold();
        }
    }

    // move to des and render center pos
    public void MoveTo(Vector3 des)
    {
        transform.position = des;
        // transform.position += des - centerPoint.position;
    }

    private void HandleMoveInputs()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log(Time.time);
            if (Movable(Vector2Int.left))
            {
                _moveTime = Time.time + moveDelay;
                MoveTo(transform.position);
                board.AddToBoard(this);
            }
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (Movable(Vector2Int.right))
            {
                _moveTime = Time.time + moveDelay;
                MoveTo(transform.position);
                board.AddToBoard(this);
            }
        }
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (Rotatable(-90))
            {
                // stepTime = Time.time + stepTime;
            }
        }
        
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (Movable(Vector2Int.down))
            {
                stepTime = Time.time + stepTime;
                
            }
        }
        
    }
    
    private bool Movable(Vector2Int translation)
    {
        var simulateBlock = board.activePiece;
        simulateBlock.transform.position += new Vector3(translation.x, translation.y, 0);
        if (!board.ValidMovement(simulateBlock))
            return false;
        return true;
    }
    
    private bool Rotatable(int rotateAngle)
    {
        var simulateBlock = this;
        simulateBlock.transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), rotateAngle);
        if (!board.ValidMovement(simulateBlock))
        {
            return false;
        }
            
        return true;
    }

    private void Hold()
    {
        if(Input.GetKey(KeyCode.C)) {
            if(!_holdInTurn)
            {
                StartCoroutine(HoldCoroutine());
            }
        }
    }

    private void Smash()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            StartCoroutine(SmashCoroutine());
            board.AddToBoard(this);
            if (board.IsFullCols())
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
        }
    }

    private void Drop()
    {
        if (Time.time > stepTime)
        {
            var simulateBlock = this;
            simulateBlock.transform.position += Vector3.down;
            if(!board.ValidMovement(simulateBlock)) 
            {
                // transform.position += Vector3.up;
                board.AddToBoard(this);
                if(board.IsFullCols()){
                    gameObject.tag = "Untagged";
                    enabled = false;
                    GameManager.Instance.GameOver();
                    return;
                }
                gameObject.tag = "Untagged";
                enabled = false;
                _spawner.Spawn();
                _holdInTurn = false; // refactor
            }
            stepTime = Time.time + stepDelay;
        }
    }
    
    private void HoldAndFall()
    {
        if(Input.GetKey(KeyCode.Space))
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
        } else if(Input.GetKey(KeyCode.C)) {
            if(!_holdInTurn)
            {
                StartCoroutine(HoldCoroutine());
            }
        } else {
            
            // if(Time.time > stepTime)
            // {
                if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                {
                    stepTime = Time.time + stepDelay;
                }
                // else {
                //     stepTime = Time.time + stepDelay;
                //     _deltaFallTime -= Time.deltaTime;
                // }
            // } else {
            if (Time.time > stepTime)
            {
                var simulateBlock = this;
                simulateBlock.transform.position += Vector3.down;
                if(!board.ValidMovement(simulateBlock)) 
                {
                    // transform.position += Vector3.up;
                    board.AddToBoard(this);
                    if(board.IsFullCols()){
                        gameObject.tag = "Untagged";
                        enabled = false;
                        GameManager.Instance.GameOver();
                        return;
                    }
                    gameObject.tag = "Untagged";
                    enabled = false;
                    _spawner.Spawn();
                    _holdInTurn = false; // refactor
                }
                stepTime = Time.time + stepDelay;
            }
                // transform.position += Vector3.down;
               
            // }   
        } 
        
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        while(Movable(Vector2Int.down))
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
