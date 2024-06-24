using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Block : MonoBehaviour
{
    public Board board { get; private set; }
    public Tetromino data { get; private set; }
    
    public Vector3Int[] tilemapCellsPositions { get; private set; }
    public Vector3Int tilemapPosition { get; private set; }
    public int rotationIndex {
        get; private set;
    }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;
    public void Initilize(Board board, Vector3Int position, Tetromino data)
    {
        this.board = board;
        this.tilemapPosition = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepTime;
        this.lockTime = 0f;
        
        
        if (tilemapCellsPositions == null)
        {
            tilemapCellsPositions = new Vector3Int[data.Cells.Length];
        }

        for (int i = 0; i < data.Cells.Length; i++)
        {
            tilemapCellsPositions[i] = (Vector3Int)data.Cells[i]; // ??
        }
    }

    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.O))
        {
            Rotate(-1); // 
        } 
        else if
        (Input.GetKeyDown(KeyCode.P))
        {
            Rotate(1); // 
        }
        
        // horizon move
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ValidateMove(Vector2Int.left);
        } 
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ValidateMove(Vector2Int.right);
        }
        
        // Soft drop
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ValidateMove(Vector2Int.down);
        }
        
        // Hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if (Time.time >= this.stepTime)
        {
            Step();
        }
        this.board.SetTiles(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        ValidateMove(Vector2Int.down);
        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }
    
    private void HardDrop()
    {
        while (ValidateMove(Vector2Int.down))
        {
            continue;
        }
    }

    private void Lock()
    {
        this.board.SetTiles(this);
        this.board.ClearLines();
        this.board.SpawnBlock();
    }
    
    
    // validate a simulate move then move if it can
    private bool ValidateMove(Vector2Int translation)
    {
        Vector3Int simulatePosition = this.tilemapPosition;
        simulatePosition.x += translation.x;
        simulatePosition.y += translation.y;

        bool isValid = this.board.IsValidPosition(this, simulatePosition);
        if (isValid)
        {   
            this.tilemapPosition = simulatePosition;
            this.lockTime = 0f;
        }

        return isValid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex += Wrap(this.rotationIndex * direction, 0, 4);
        ApplyRotationMatrix(direction);

        // reverse if false
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.tilemapCellsPositions.Length; i++)
        {
            Vector3 cell = this.tilemapCellsPositions[i];
            int x, y;
            switch (this.data.type)
            {
                // base on https://en.wikipedia.org/wiki/Rotation_matrix
                case TetrominoType.I:
                case TetrominoType.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.RoundToInt(cell.x * Data.RotationMatrix[0] * direction + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt(cell.x * Data.RotationMatrix[2] * direction + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt(cell.x * Data.RotationMatrix[0] * direction + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt(cell.x * Data.RotationMatrix[2] * direction + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.tilemapCellsPositions[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndexFunc, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndexFunc, rotationDirection);
        for (int i = 0; i < this.data.WallKicks.GetLength(i); i++)
        {
            Vector2Int translation = this.data.WallKicks[wallKickIndex, i];
            if (ValidateMove(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.WallKicks.Length);
    }
    
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return max + (input - min) % (max - min);
        }
    }
}
