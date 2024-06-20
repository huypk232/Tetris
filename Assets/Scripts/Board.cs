using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private BlockType type;
    [SerializeField] private Transform rotationPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Material tilePrefab;

    public readonly Transform[] Tiles = new Transform[(Constants.RightLimit - Constants.LeftLimit) * (Constants.TopLimit - Constants.BottomLimit)]; // use 1 dimension array to optimize speed
    private static GameObject _heldBlock;
    private static bool _holdInTurn;
    
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
