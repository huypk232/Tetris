using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private TetrominoType type;
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material tilePrefab;

    public readonly Transform[] Tiles = new Transform[(Constants.RightLimit - Constants.LeftLimit) * (Constants.TopLimit - Constants.BottomLimit)]; // use 1 dimension array to optimize speed
    private static GameObject _heldBlock;
    private static bool _holdInTurn;
    
    [Tooltip("I_Block, J_Block, L_Block, O_Block, S_Block, T_Block, Z_Block")]
    public GameObject[] blocks;
    public Transform[] nextAreas;
    public Transform holdArea;

    private const int TotalBlocks = 7;
    
    private readonly List<GameObject> _nextBlocks = new();
    private GameObject _holdBlock;
    private Shadow _shadow;
    private readonly int[] _spawnCounter = new int[TotalBlocks];
    private int _remainBlockInCycle;
    
    private void Start()
    {
        InitSpawnCounter();
        
        int firstRandomIndex = Random.Range(0, blocks.Length);
        _spawnCounter[firstRandomIndex]--;
        _remainBlockInCycle--;
        
        Block firstBlock = Instantiate(blocks[firstRandomIndex], transform.position, Quaternion.identity).GetComponent<Block>();
        firstBlock.SetBoard(this);
        _shadow.Follow(firstBlock);
        
        while(_nextBlocks.Count < 5)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, blocks.Length);
            } while (_spawnCounter[randomIndex] == 0);

            _spawnCounter[randomIndex]--;
            _remainBlockInCycle--;
            GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[_nextBlocks.Count].position, Quaternion.identity);
            if(newBlock.TryGetComponent(out Block block))
            {
                block.MoveTo(nextAreas[_nextBlocks.Count].position);
                block.enabled = false;
            }
            _nextBlocks.Add(newBlock);
        }
    }

    private void InitSpawnCounter()
    {
        for (int i = 0; i < _spawnCounter.Length; i++)
        {
            var randomQuantity = Random.Range(1, 3);
            _spawnCounter[i] = randomQuantity;
            _remainBlockInCycle += randomQuantity;
        }
              
    }

    public void Spawn()
    {
        var oldPosition = _nextBlocks[0].transform.position;
        _nextBlocks[0].transform.position = transform.position;
        if( _nextBlocks[0].TryGetComponent(out Block nextBlock))
        {
            if(this.ValidMovement(nextBlock)){
                nextBlock.enabled = true;
                nextBlock.SetBoard(this);
                _shadow.Follow(nextBlock);
            } else {
                _nextBlocks[0].transform.localPosition = oldPosition;
                GameManager.Instance.GameOver();
                return;
            }
        }
        _nextBlocks.RemoveAt(0);
        UpdateNextBlocks();
    }

    private void UpdateNextBlocks()
    {
        // change position block in next area
        for (int i = 0; i < _nextBlocks.Count; i++)
        {
            if(_nextBlocks[i].TryGetComponent(out Block nextBlock))
            {
                nextBlock.MoveTo(nextAreas[i].position);
            }
        }

        if (_remainBlockInCycle <= 0)
            InitSpawnCounter();
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, blocks.Length);
        } while (_spawnCounter[randomIndex] == 0);

        _spawnCounter[randomIndex]--;
        _remainBlockInCycle--;
        
        // spawn new block to next area
        GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[4].position, Quaternion.identity);


        if(newBlock.TryGetComponent(out Block block))
        {
            block.MoveTo(nextAreas[4].position);
            block.enabled = false;
        }
        _nextBlocks.Add(newBlock);
    }
    
    public void AddToBoard(Block block)
    {
        var minY = Constants.TopLimit;
        var maxY = Constants.BottomLimit;
        foreach (Transform child in block.gameObject.transform)
        {
            if(child.gameObject.CompareTag("CenterPoint")) 
                continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            Tiles[GetIndexOnBoardTiles(xIndex, yIndex)] = child;
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
        for (int column = Constants.LeftLimit; column < Constants.RightLimit - Constants.LeftLimit; column++)
        {
            if (!Tiles[GetIndexOnBoardTiles(column, y)])
                return false;
        }
        return true;
    }

    private void DeleteFullRow(int y)
    {
        for (int x = 0; x < Constants.RightLimit; x++)
        {
            Destroy(Tiles[GetIndexOnBoardTiles(x, y)].gameObject);
            Tiles[GetIndexOnBoardTiles(x, y)] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < Constants.TopLimit; y++)
        {
            for (int x = Constants.LeftLimit; x < Constants.RightLimit - Constants.LeftLimit; x++)
            {
                if(Tiles[GetIndexOnBoardTiles(x, y)])
                {
                    Tiles[GetIndexOnBoardTiles(x, y - 1)] = Tiles[GetIndexOnBoardTiles(x, y)];
                    Tiles[GetIndexOnBoardTiles(x, y)] = null;
                    Tiles[GetIndexOnBoardTiles(x, y - 1)].position += Vector3.down;
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
            if(child.position.x < Constants.LeftLimit || child.position.x > Constants.RightLimit || child.position.y <= Constants.BottomLimit)
            {
                return false;
            }

            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            if(Tiles[GetIndexOnBoardTiles(xIndex, yIndex)]) 
                return false;
        }
        return true;
    }

    private static int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (Constants.RightLimit - Constants.LeftLimit) + column;
    }
}
