using System.Collections;
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
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material tilePrefab;
    
    
    private const float FallTime = 1f;
    private float _deltaFallTime;

    
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    private Board _board;
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

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Move)
        {
            Move();
            HoldAndFall();
            // Hold();
        }
    }

    public void SetBoard(Board board)
    {
        _board = board;
    }

    public Transform GetRotationPoint()
    {
        return rotationPoint;
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
            if(!_board.ValidMovement(this)) transform.position += Vector3.right;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            if(!_board.ValidMovement(this)) transform.position += Vector3.left;
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), -90);
            if(!_board.ValidMovement(this)) 
                transform.RotateAround(rotationPoint.position, new Vector3(0, 0, 1), 90);

        }  
    }

    private void HoldAndFall()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SmashCoroutine());
            _board.AddToBoard(this);
            if(_board.IsFullCols(this))
            {
                enabled = false;
                GameManager.Instance.GameOver();
                return;
            }
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
                    _deltaFallTime -= 10 * Time.deltaTime;
                } else {
                    _deltaFallTime -= Time.deltaTime;
                }
            } else {
                transform.position += Vector3.down;
                if(!_board.ValidMovement(this)) 
                {
                    transform.position += Vector3.up;
                    _board.AddToBoard(this);
                    if(_board.IsFullCols(this)){
                        enabled = false;
                        GameManager.Instance.GameOver();
                        return;
                    }
                    enabled = false;
                    _spawner.Spawn();
                    _holdInTurn = false; // refactor
                }
                _deltaFallTime = FallTime;
            }   
        } 
        
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        while(_board.ValidMovement(this))
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
}
