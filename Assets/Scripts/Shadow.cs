using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Shadow : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Block simulateBlock;
    
    public Tilemap Tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    public void Awake()
    {
        this.Tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
       Clear();
       Copy();
       Drop();
       SetTiles();
    }

    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            Tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.simulateBlock.tilemapCellsPositions[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = this.simulateBlock.tilemapPosition;
        int currentRow = position.y;
        int bottomRow = -this.board.boardSize.y / 2 - 1;
        
        this.board.Clear(this.simulateBlock);
        for (int row = currentRow; row >= bottomRow; row--)
        {
            position.y = row;
            if (this.board.IsValidPosition(this.simulateBlock, position))
            {
                this.position = position;
            }
            else
            {
                break;
            }
            
        }
        this.board.SetTiles(this.simulateBlock);
    }

    public void SetTiles()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            Tilemap.SetTile(tilePosition, this.tile);
        }
    }
}

    