using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Tooltip("I_Block, J_Block, L_Block, O_Block, S_Block, T_Block, Z_Block")]
    public GameObject[] blocks;
    public Transform[] nextAreas;
    public Transform holdArea;

    private List<GameObject> _nextBlocks = new List<GameObject>();
    private GameObject _holdBlock;

    public bool holdInTurn = false;

    void Start()
    {
        int firstRandomIndex = Random.Range(0, blocks.Length);
        Instantiate(blocks[firstRandomIndex], transform.position, Quaternion.identity);

        // Spawn block in next area
        while(_nextBlocks.Count < 5)
        {
            int randomIndex = Random.Range(0, blocks.Length);
            GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[_nextBlocks.Count].position, Quaternion.identity);
            if(newBlock.TryGetComponent(out Block block))
            {
                block.MoveTo(nextAreas[_nextBlocks.Count].position);
                block.enabled = false;
            }
            _nextBlocks.Add(newBlock);
        }
    }

    public void Spawn()
    {
        var oldPosition = _nextBlocks[0].transform.position;
        _nextBlocks[0].transform.position = transform.position;
        if( _nextBlocks[0].TryGetComponent(out Block nextBlock))
        {
            if(nextBlock.ValidMovement()){
                nextBlock.enabled = true;
            } else {
                _nextBlocks[0].transform.localPosition = oldPosition;
                GameManager.Instance.GameOver();
                return;
            }
        }
        _nextBlocks.RemoveAt(0);
        UpdateNextBlocks();
    }

    private void UpdateNextBlocks()
    {
        // change position block in next area
        for (int i = 0; i < _nextBlocks.Count; i++)
        {
            if(_nextBlocks[i].TryGetComponent(out Block nextBlock))
            {
                nextBlock.MoveTo(nextAreas[i].position);
            }
        }

        // spawn new block to next area
        int randomIndex = Random.Range(0, blocks.Length);
        GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[4].position, Quaternion.identity);


        if(newBlock.TryGetComponent(out Block block))
        {
            block.MoveTo(nextAreas[4].position);
            block.enabled = false;
        }
        _nextBlocks.Add(newBlock);
    }

}
