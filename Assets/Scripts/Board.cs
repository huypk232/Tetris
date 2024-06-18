using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    
    // public Tilemap tilemap { get; private set; }
    public Block activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    
    public RectInt Bounds {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }
    
    // start here
    private Transform[] _tiles = new Transform[(RightLimit - LeftLimit) * (TopLimit - BottomLimit)];

    private void Awake()
    {
        // tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Block>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    // private void Start()
    // {
    //     SpawnPiece();
    // }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];
    
        // activePiece.Initialize(this, spawnPosition, data);
        //
        // if (ValidMovement(activePiece)) {
        //     AddToBoard(activePiece);
        // } else {
        //     GameOver();
        // }
    }

    public void ChangeActiveBlock(Block block)
    {
        // if (!activePiece)
        // {
            activePiece = block;
            activePiece.gameObject.tag = "OnBoardBlock";
            // }
    }
    
    public bool ValidMovement(Block simulateBlock)
    {
        foreach (Transform child in simulateBlock.transform)
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

    private int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (RightLimit - LeftLimit) + column;
    }
    
    public void AddToBoard(Block block)
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        Debug.Log(block.gameObject.transform.childCount);
        foreach (Transform child in block.transform)
        {
            Debug.Log(child.gameObject.name);
            Debug.Log(block.gameObject.transform.position);
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

        // activePiece.gameObject.tag = "Untagged";
        // activePiece.enabled = false;
        // activePiece = null;
    }

    public bool IsFullCols()
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
            if (!_tiles[GetIndexOnBoardTiles(column, y)])
                return false;
        }
        return true;
    }

    private void DeleteFullRow(int row)
    {
        for (int x = 0; x < RightLimit - LeftLimit; x++)
        {
            _tiles[GetIndexOnBoardTiles(x, row)] = null;
            // Board[GetIndexOnBoardTiles(x, y)] = null;
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
                    // tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    // tilemap.SetTile(new Vector3Int(x, y - 1, 0), tilemap.GetTile(new Vector3Int(x, y, 0)));
                    // Board[GetIndexOnBoardTiles(x, y)] = null;
                    // Board[GetIndexOnBoardTiles(x, y - 1)].position += Vector3.down;
                }
            }
        }
    }
}
