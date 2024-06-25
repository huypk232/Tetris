using System.Collections.Generic;
using UnityEngine;

// ref: https://tetris.fandom.com/wiki/SRS
public static class Data
{
    public static readonly float cos = Mathf.Cos(Mathf.PI / 2f);
    public static readonly float sin = Mathf.Sin(Mathf.PI / 2f);
    public static readonly float[] RotationMatrix = new  float[]{ cos, sin, -sin, cos};

    // struct tile base render
    public static readonly Dictionary<TetrominoType, Vector2Int[]> Cells = new Dictionary<TetrominoType, Vector2Int[]>()
    {
        {
            TetrominoType.I,
            new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) }
        },
        {
            TetrominoType.J,
            new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) }
        },
        {
            TetrominoType.L,
            new[] { new Vector2Int(1, 1), new Vector2Int(1, 1), new Vector2Int(0, 0), new Vector2Int(-1, 0) }
        },
        {
            TetrominoType.O,
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) }
        },
        {
            TetrominoType.S,
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0) }
        },
        {
            TetrominoType.T,
            new[] { new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) }
        },
        {
            TetrominoType.Z,
            new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) }
        },
    };

    // solve rotation when I tetromium beside the left and right board limit
    public static readonly Vector2Int[,] TetrominoIWallKicks = new Vector2Int[,]
    {
        {
            new (0, 0), new (-2, 0), new (1, 0), new (-2, -1), new(1, 2),
        },
        {
            new (0, 0), new ( 2, 0), new (-1, 0), new ( 2, 1), new(-1, -2)
        },
        {
            new (0, 0), new (-1, 0), new ( 2, 0), new(-1, 2), new( 2,-1)
        },
        {
            new (0, 0), new ( 1, 0), new (-2, 0), new (1,-2), new (-2, 1)
        },
        {
            new (0, 0), new( 2, 0), new	(-1, 0), new ( 2, 1), new (-1,-2)
        },
        {
            new (0, 0), new (-2, 0), new ( 1, 0), new (-2,-1), new ( 1, 2)
        },
        {
            new (0, 0), new (1, 0), new (-2, 0), new ( 1,-2), new (-2, 1)
        },
        {
            new (0, 0), new (-1, 0), new ( 2, 0), new (-1, 2), new ( 2,-1)
        },
    };

    public static readonly Vector2Int[,] TetrominoJLTSZWallKicks = new Vector2Int[,]
    {
        {
            new (0, 0), new (-1, 0), new (-1, 1), new (0, -2), new(-1, -2),
        },
        {
            new (0, 0), new (1, 0), new (1, -1), new (0, 2), new(1, 2)
        },
        {
            new (0, 0), new (1, 0), new (1, -1), new(0, 2), new(1, 2)
        },
        {
            new (0, 0), new (-1, 0), new (-1, 1), new (0, -2), new (-1, -2)
        },
        {
            new (0, 0), new(1, 0), new	(1, 1), new (0, -2), new (1,-2)
        },
        {
            new (0, 0), new (-1, 0), new (-1, -1), new (0, 2), new (-1, 2)
        },
        {
            new (0, 0), new (-1, 0), new (-1, -1), new (0, 2), new (-1, 2)
        },
        {
            new (0, 0), new (1, 0), new (1, 1), new (0, -2), new (1, -2)
        },
    };
    
    public static readonly Dictionary<TetrominoType, Vector2Int[,]> WallKicks = new Dictionary<TetrominoType, Vector2Int[,]>()
    {
        {
            TetrominoType.I, TetrominoIWallKicks},
        {TetrominoType.L, TetrominoJLTSZWallKicks},
            {TetrominoType.J, TetrominoIWallKicks},
            {TetrominoType.O, TetrominoIWallKicks},
            {TetrominoType.S, TetrominoIWallKicks},
            {TetrominoType.T, TetrominoIWallKicks},
            {TetrominoType.Z, TetrominoIWallKicks
        },
    };
}
