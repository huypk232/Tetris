using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

// public enum TetrominoType {
//     I = 1,
//     J = 2,
//     L = 3,
//     O = 4,
//     S = 5,
//     T = 6,
//     Z = 7,
// }

public enum TetrominoType
{
    I, J, L, O, S, T, Z
}

[System.Serializable]
public class TetrominoData
{
    public Tile tile;
    [FormerlySerializedAs("tetromino")] public TetrominoType tetrominoType;

    public Vector2Int[] Cells { get; private set; }

    public void Initialize()
    {
        Cells = BlockData.Childs[tetrominoType];
    }
}
