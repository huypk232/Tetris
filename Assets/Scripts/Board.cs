using System.Collections;
using Unity.VisualScripting;
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

    [Header("Share object")]
    private static Transform[][] _tiles = new Transform[RightLimit - LeftLimit][];
    private static GameObject _heldBlock;
    private static bool _holdInTurn;

    [SerializeField] private Transform holdArea;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Spawner spawner;
    
    [Tooltip("Offset center to modify when rendering")]
    [SerializeField] private Vector3 centerOffset;

    private Block _onBoardBlock;

    private void Awake() {
        for (int column = 0; column < RightLimit - LeftLimit; column++)
        {
            _tiles[column] = new Transform[TopLimit - BottomLimit];
        }
    }

    private void Start()
    {
        holdArea = GameObject.Find("/Level/Hold Area/Hold").transform;
        spawnPoint = GameObject.Find("/Level/Spawner").transform;
        spawner = FindObjectOfType<Spawner>();
        _onBoardBlock = GameObject.FindGameObjectWithTag("OnBoard").GetComponent<Block>();
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

    private void Move()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // _onBoardBlock.gameObject.transform.position += Vector3.left;
            if (ValidMovement(_onBoardBlock, Vector3.left)) _onBoardBlock.gameObject.transform.position += Vector3.left;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // _onBoardBlock.gameObject.transform.position += Vector3.right;
            if (ValidMovement(_onBoardBlock, Vector3.right)) _onBoardBlock.gameObject.transform.position += Vector3.right;
        }

        // todo
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            _onBoardBlock.gameObject.transform.RotateAround(_onBoardBlock.rotationPoint.position, new Vector3(0, 0, 1), -90);
            if(!ValidMovement(_onBoardBlock, Vector3.zero)) 
                _onBoardBlock.gameObject.transform.RotateAround(_onBoardBlock.rotationPoint.position, new Vector3(0, 0, 1), 90);

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
                _onBoardBlock.enabled = false;
                GameManager.instance.GameOver();
                return;
            }
            _onBoardBlock.enabled = false;
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
                    _deltaFallTime -= 20 * Time.deltaTime;
                } else {
                    _deltaFallTime -= Time.deltaTime;
                }
            } else {
                // _onBoardBlock.gameObject.transform.position += Vector3.down;
                if(ValidMovement(_onBoardBlock, Vector3.down)) 
                {
                    _onBoardBlock.gameObject.transform.position += Vector3.down;
                    
                    AddToBoard();
                    if(IsFullCols()){
                        _onBoardBlock.enabled = false;
                        GameManager.instance.GameOver();
                        return;
                    }
                    _onBoardBlock.enabled = false;
                    spawner.Spawn();
                    _holdInTurn = false; // refactor
                }
                _deltaFallTime = FallTime;
            }   
        } 
        
    }

    public void SetOnBoardBlock(ref Block block)
    {
        if (block)
            block.tag = "OnBoard";
        // Destroy(_onBoardBlock.gameObject);
        _onBoardBlock = block;
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.instance.currentState = GameState.Wait;
        while(ValidMovement(_onBoardBlock, Vector3.down))
        {
            _onBoardBlock.gameObject.transform.position += Vector3.down;
        }
        // _onBoardBlock.gameObject.transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.currentState = GameState.Move;
    }

    // todo do not need this, remove
    private IEnumerator HoldCoroutine()
    {
        GameManager.instance.currentState = GameState.Wait;
        _onBoardBlock.enabled = false;
        _onBoardBlock.gameObject.transform.rotation = Quaternion.identity;
        _onBoardBlock.RenderCenter(holdArea.position);

        if(!_heldBlock)
        {
            _heldBlock = _onBoardBlock.gameObject;
            spawner.Spawn();
        } else {
            _heldBlock.TryGetComponent(out Block tempBlock);
            {
                tempBlock.enabled = true;
            }
            _heldBlock.transform.position = spawnPoint.transform.position;
            _heldBlock = _onBoardBlock.gameObject;
        }
        _holdInTurn = true;
        yield return null;
        GameManager.instance.currentState = GameState.Move;
    }

    private void AddToBoard()
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        Debug.Log("Maybe null: " + _onBoardBlock);
        Debug.Log("Maybe transform null: " + _onBoardBlock.gameObject);
        foreach (Transform child in _onBoardBlock.gameObject.transform)
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
        foreach(Transform child in _onBoardBlock.gameObject.transform)
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
        for (int x = 0; x < RightLimit - LeftLimit; x++)
        {
            if (!_tiles[x][y])
                return false;
        }
        return true;
    }

    private static void DeleteFullLine(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            Destroy(_tiles[x][y].gameObject);
            _tiles[x][y] = null;
        }
    }

    private static void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = 0; x < RightLimit; x++)
            {
                if(_tiles[x][y])
                {
                    _tiles[x][y - 1] = _tiles[x][y];
                    _tiles[x][y] = null;
                    _tiles[x][y - 1].position += Vector3.down;
                }
            }
        }
    }

    public bool ValidMovement(Block block, Vector3 offsetPosition)
    {
        foreach (Transform child in block.gameObject.transform)
        {
            if (child.position.x < LeftLimit || child.position.x > RightLimit || child.position.y <= BottomLimit)
            {
                return false;
            }

            int xIndex = (int)(child.position.x + offsetPosition.x);
            int yIndex = (int)(child.position.y + offsetPosition.y);
            if (_tiles[xIndex][yIndex])
                return false;
        }

        return true;
    }
}
