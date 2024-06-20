using System.Collections;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] private Board board;

    [SerializeField] private GameObject tilePrefab;
    
    private Block _followBlock;
    private Transform _rotationPoint;
    private Vector3 _spawnerPosition;

    public void SetFollowBlock(Block block, Vector3 spawnerPosition)
    {
        _spawnerPosition = spawnerPosition;
        ToOriginal();
        Copy(block);
    }
    
    private void LateUpdate()
    {
        if (GameManager.Instance.currentState == GameState.Move)
            Move();
    }

    private void Copy(Block block)
    {
        _followBlock = block;
        transform.position = block.transform.position;
        _rotationPoint = block.GetRotationPoint();
        foreach (Transform child in block.gameObject.transform)
        {
            if (!child.gameObject.CompareTag("CenterPoint"))
            {
                var shadowTile = Instantiate(tilePrefab, child.transform.position, Quaternion.identity, transform);
                if (_rotationPoint.position == child.position)
                    _rotationPoint = shadowTile.transform;
            }
        }

        StartCoroutine(SmashCoroutine());
    }

    private void ToOriginal()
    {
        transform.position = Vector3.zero;
        _rotationPoint = null;
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);

        }
    }

    public void RePosition()
    {
        transform.position = _followBlock.transform.position;
        StartCoroutine(SmashCoroutine());
    }
    
    private void Move()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left;
            transform.position = new Vector3(transform.position.x, _spawnerPosition.y, transform.position.z);
            if(!ValidMovement()) transform.position += Vector3.right;
            // StartCoroutine(SmashCoroutine());
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += Vector3.right;
            transform.position = new Vector3(transform.position.x, _spawnerPosition.y, transform.position.z);
            if(!ValidMovement()) transform.position += Vector3.left;
            // StartCoroutine(SmashCoroutine());
        }

        if(Input.GetKeyDown(KeyCode.UpArrow) && _rotationPoint)
        {
            transform.position = new Vector3(transform.position.x, _spawnerPosition.y, transform.position.z);
            transform.RotateAround(_rotationPoint.position, new Vector3(0, 0, 1), -90);
            if (!ValidMovement())
            {
                transform.RotateAround(_rotationPoint.position, new Vector3(0, 0, 1), 90);
            }
            // StartCoroutine(SmashCoroutine());
        }
        Smash();
    }

    private IEnumerator SmashCoroutine()
    {
        GameManager.Instance.currentState = GameState.Wait;
        while(ValidMovement())
        {
            transform.position += Vector3.down;
        }
        transform.position += Vector3.up;
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.currentState = GameState.Move;
    }

    private bool ValidMovement()
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
            if (board.Tiles[GetIndexOnBoardTiles(xIndex, yIndex)])
            {
                return false;
            }
        }
        return true;
    }
    
    private static int GetIndexOnBoardTiles(int column, int row)
    {
        return row * (Constants.RightLimit - Constants.LeftLimit) + column;
    }

    private void Smash()
    {
        while(ValidMovement())
        {
            transform.position += Vector3.down;
        }
        transform.position += Vector3.up;
    }
}