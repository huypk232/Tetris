using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    
    public Tilemap tilemap { get; private set; }
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

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Block>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];
    
        activePiece.Initialize(this, spawnPosition, data);
    
        if (ValidMovement(activePiece)) {
            AddToBoard(activePiece);
        } else {
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();

        // Do anything else you want on game over here..
    }

    // public void Set(Piece piece)
    // {
    //     for (int i = 0; i < piece.cells.Length; i++)
    //     {
    //         Vector3Int tilePosition = piece.cells[i] + piece.position;
    //         tilemap.SetTile(tilePosition, piece.data.tile);
    //     }
    // }
    //
    // public void Clear(Piece piece)
    // {
    //     for (int i = 0; i < piece.cells.Length; i++)
    //     {
    //         Vector3Int tilePosition = piece.cells[i] + piece.position;
    //         tilemap.SetTile(tilePosition, null);
    //     }
    // }

    // public bool IsValidPosition(Piece piece, Vector3Int position)
    // {
    //     RectInt bounds = Bounds;
    //
    //     // The position is only valid if every cell is valid
    //     for (int i = 0; i < piece.cells.Length; i++)
    //     {
    //         Vector3Int tilePosition = piece.cells[i] + position;
    //
    //         // An out of bounds tile is invalid
    //         if (!bounds.Contains((Vector2Int)tilePosition)) {
    //             return false;
    //         }
    //
    //         // A tile already occupies the position, thus invalid
    //         if (tilemap.HasTile(tilePosition)) {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
            } else {
                row++;
            }
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
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
            if(tilemap.HasTile(new Vector3Int(xIndex, yIndex, 0))) 
                return false;
        }
        return true;
    }

    private static int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (RightLimit - LeftLimit) + column;
    }
    
    public void AddToBoard(Block block)
    {
        var minY = TopLimit;
        var maxY = BottomLimit;
        foreach (Transform child in transform)
        {
            if(child.gameObject.CompareTag("CenterPoint")) 
                continue;
            var xIndex = (int)child.position.x;
            var yIndex = (int)child.position.y;
            tilemap.SetTile(new Vector3Int(xIndex, yIndex, 0), block.data.tile);
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
            if (!tilemap.HasTile(new Vector3Int(column, y, 0)))
                return false;
        }
        return true;
    }

    private void DeleteFullRow(int y)
    {
        for (int x = 0; x < RightLimit; x++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), null);
            // Board[GetIndexOnBoardTiles(x, y)] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TopLimit; y++)
        {
            for (int x = LeftLimit; x < RightLimit - LeftLimit; x++)
            {
                if(tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    tilemap.SetTile(new Vector3Int(x, y - 1, 0), tilemap.GetTile(new Vector3Int(x, y, 0)));
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    tilemap.SetTile(new Vector3Int(x, y - 1, 0), tilemap.GetTile(new Vector3Int(x, y, 0)));
                    // Board[GetIndexOnBoardTiles(x, y)] = null;
                    // Board[GetIndexOnBoardTiles(x, y - 1)].position += Vector3.down;
                }
            }
        }
    }
}
