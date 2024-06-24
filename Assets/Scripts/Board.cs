using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Board : MonoBehaviour
{
    public Tilemap Tilemap { get; private set; }
    public Block ActiveBlock { get; private set; }
    public Tetromino[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20); // top right

    public RectInt Bounds
    {
        get
        {
            // bottom left
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }
    
    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActiveBlock = GetComponentInChildren<Block>();
        foreach (var t in tetrominoes)
        {
            t.Initialize();
        }
    }

    private void Start()
    {
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        int random = Random.Range(0, tetrominoes.Length);
        Tetromino data = tetrominoes[random];
        
        ActiveBlock.Initilize(this, spawnPosition, data);

        if (IsValidPosition(this.ActiveBlock, this.spawnPosition))
        {
            SetTiles(this.ActiveBlock);
        }
        else
        {
            // GameManager.Instance.GameOver();
            // this.Tilemap.ClearAllTiles();
        }
    }

    public void SetTiles(Block block)
    {
        for (int i = 0; i < block.tilemapCellsPositions.Length; i++)
        {
            Vector3Int tilePosition = block.tilemapCellsPositions[i] + block.tilemapPosition;
            Tilemap.SetTile(tilePosition, block.data.tileBase);
        }
    }
    
    // todo refactor
    public void Clear(Block block)
    {
        for (int i = 0; i < block.tilemapCellsPositions.Length; i++)
        {
            Vector3Int tilePosition = block.tilemapCellsPositions[i] + block.tilemapPosition;
            Tilemap.SetTile(tilePosition, null);
        }
    }
    
    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        
        for (int i = 0; i < block.tilemapCellsPositions.Length; i++)
        {
            Vector3Int tilePosition = block.tilemapCellsPositions[i] + position;
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.Tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                ClearSingleLine(row);
            }
            else
            {
                row++;
            }
        }
    }

    public void ClearSingleLine(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.Tilemap.SetTile(position, null);
        }
    }
    

    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!this.Tilemap.HasTile(position))
            {
                return false;
            }
            
        }

        RowDown(row);

        return true;
    }

    private void RowDown(int row)
    {
        RectInt bounds = this.Bounds;

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.Tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.Tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
