using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 rotationPoint;
    private static int leftLimit = 0;
    private static int rightLimit = 10;
    private static int botLimit = 0;
    private static int topLimit = 23;

    private float _fallTime = 1f;
    private float _deltaFallTime;

    private static Transform[,] board = new Transform[rightLimit, topLimit];
    private static GameObject heldBlock;
    private static bool holdInTurn = false;

    private Transform holdArea;
    private Transform spawnPoint;

    [Tooltip("Offset center to modify when rendering")]
    public Vector3 center;

    void Start()
    {
        holdArea = GameObject.Find("/Level/Hold Area/Hold").transform;
        spawnPoint = GameObject.Find("/Level/Spawner").transform;
        _deltaFallTime = _fallTime;
    }

    void Update()
    {
        Move();
        Hold();
        Fall();
    }

    // render center to destination des
    public void RenderCenter(Vector3 des)
    {
        transform.position = des;
        transform.position += des - transform.TransformPoint(center);
    }

    private void Move()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;
            if(!ValidMovement()) transform.position += Vector3.right;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            if(!ValidMovement()) transform.position += Vector3.left;
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            if(!ValidMovement()) 
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);

        }  
    }

    private void Fall()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            while(ValidMovement())
            {
                transform.position += Vector3.down;
            }
            transform.position += Vector3.up;
            AddToBoard();
            if(IsFullCols())
            {
                enabled = false;
                GameManager.instance.GameOver();
                return;
            }
            enabled = false;
            FindObjectOfType<Spawner>().Spawn();
            holdInTurn = false; // refactor
        }
        
        if(_deltaFallTime > 0.0f)
        {
            if(Input.GetKey(KeyCode.DownArrow))
            {
                _deltaFallTime -= 10 * Time.deltaTime;
            } else {
                _deltaFallTime -= Time.deltaTime;
            }
        } else {
            transform.position += Vector3.down;
            if(!ValidMovement()) 
            {
                transform.position += Vector3.up;
                AddToBoard();
                if(IsFullCols()){
                    enabled = false;
                    GameManager.instance.GameOver();
                    return;
                }
                enabled = false;
                FindObjectOfType<Spawner>().Spawn();
                holdInTurn = false; // refactor
            }
            _deltaFallTime = _fallTime;
        }   
    }

    private void Hold()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(holdInTurn);
            if(!holdInTurn)
            {
                enabled = false;
                transform.rotation = Quaternion.identity;
                RenderCenter(holdArea.position);

                if(heldBlock == null)
                {
                    heldBlock = transform.gameObject;
                    FindObjectOfType<Spawner>().Spawn();
                } else {
                    heldBlock.TryGetComponent<Block>(out Block tempBlock);
                    {
                        tempBlock.enabled = true;
                    }
                    heldBlock.transform.position = spawnPoint.position;
                    heldBlock = transform.gameObject;
                }
                holdInTurn = true;
            } else return;
        }
    }

    private void AddToBoard()
    {
        int minY, maxY;
        minY = topLimit;
        maxY = botLimit;
        foreach (Transform child in transform)
        {
            if(child.name == "Center") continue;
            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            board[xIndex, yIndex] = child;
            if (minY > yIndex) minY = yIndex;
            else if (maxY < yIndex) maxY = yIndex;
        }
        // check lines is full ?, todo refactor

        for(int line = maxY; line >= minY; line--)
        {
            if(IsFullLine(line))
            {
                DeleteFullLine(line);
                RowDown(line);
            }
        }
    }

    private bool IsFullCols()
    {
        foreach(Transform child in transform)
        {
            int yIndex = (int)child.position.y;

            if(yIndex > 20)
                return true;
        }
        return false;
    }

    private void CheckLines()
    {
        for(int i = topLimit-1; i >= 0; i--)
        {
            if(IsFullLine(i))
            {
                DeleteFullLine(i);
                RowDown(i);
            }
        }
    }

    private bool IsFullLine(int y)
    {
        for (int x = 0; x < rightLimit; x++)
        {
            if (board[x, y] == null)
                return false;
        }
        return true;
    }

    private void DeleteFullLine(int y)
    {
        for (int x = 0; x < rightLimit; x++)
        {
            Destroy(board[x, y].gameObject);
            board[x, y] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < topLimit; y++)
        {
            for (int x = 0; x < rightLimit; x++)
            {
                if(board[x, y] != null)
                {
                    board[x, y - 1] = board[x, y];
                    board[x, y] = null;
                    board[x, y - 1].position += Vector3.down;
                }
            }
        }
    }

    public bool ValidMovement()
    {
        foreach (Transform child in transform)
        {
            if(child.position.x < leftLimit || child.position.x > rightLimit || child.position.y <= botLimit)
            {
                return false;
            }

            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            if(board[xIndex, yIndex] != null)
                return false;
        }
        return true;
    }
}
