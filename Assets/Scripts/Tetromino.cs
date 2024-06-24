using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TetrominoType
{
    I,
    J,
    L,
    O,
    S,
    T,
    Z,
}

[Serializable]
public class Tetromino
{
    public TetrominoType type;

    public Tile tileBase;
    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }
    

    public void Initialize()
    {
        Cells = Data.Cells[type];
        WallKicks = Data.WallKicks[this.type];
    }
}
