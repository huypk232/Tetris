using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Data
{
    [CreateAssetMenu(fileName = "TetrominoType", menuName = "TETROMINO", order = 0)]
    public class Tetromino : ScriptableObject
    {
        public Tile tile;
        public TetrominoType type;

        public Vector2Int[] Cells { get; private set; }
    }
}