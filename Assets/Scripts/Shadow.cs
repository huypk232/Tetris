using System;
using System.Collections;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] private Board board;

    [SerializeField] private GameObject tilePrefab;
    
    private Block _followBlock;
    private Transform _rotationPoint;

    public void SetFollowBlock(Block block)
    {
        Copy(block);
    }
    
    private void Update()
    {
        Move();
    }

    public void Copy(Block block)
    {
        ToOriginal();
        _followBlock = block;
        foreach (Transform child in block.gameObject.transform)
        {
            if(!child.gameObject.CompareTag("CenterPoint"))
                Instantiate(tilePrefab, child.transform.position, Quaternion.identity, transform);
        }

        _rotationPoint = block.GetRotationPoint();
        StartCoroutine(SmashCoroutine());

    }

    public void ToOriginal()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void Move()
    {
        bool hasMove = false;
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            hasMove = true;
            transform.position += Vector3.left;
            if(!ValidMovement()) transform.position += Vector3.right;
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            hasMove = true;
            transform.position += Vector3.right;
            if(!ValidMovement()) transform.position += Vector3.left;
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            hasMove = true;
            transform.position += Vector3.up * 2;
            transform.RotateAround(_rotationPoint.position, new Vector3(0, 0, 1), -90);
            if(!ValidMovement())
                transform.RotateAround(_rotationPoint.position, new Vector3(0, 0, 1), 90);

        }

        if (hasMove)
        {
            Copy(_followBlock);
        }
    }

    private IEnumerator SmashCoroutine()
    {
        // GameManager.Instance.currentState = GameState.Wait;
        while(ValidMovement())
        {
            transform.position += Vector3.down;
        }
        transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        // GameManager.Instance.currentState = GameState.Move;
    }
    
    public bool ValidMovement()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("CenterPoint"))
                continue;
            if(child.position.x < Constants.LeftLimit || child.position.x > Constants.RightLimit || child.position.y <= Constants.BottomLimit)
            {
                return false;
            }

            int xIndex = (int)child.position.x;
            int yIndex = (int)child.position.y;
            if(board._tiles[GetIndexOnBoardTiles(xIndex, yIndex)]) 
                return false;
        }
        return true;
    }
    
    private static int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (Constants.RightLimit - Constants.LeftLimit) + column;
    }
}