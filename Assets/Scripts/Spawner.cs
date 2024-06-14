using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Tooltip("I_Block, J_Block, L_Block, O_Block, S_Block, T_Block, Z_Block")]
    public GameObject[] blocks;
    public Transform[] nextAreas;
    public Transform holdArea;

    private List<GameObject> nextBlocks = new List<GameObject>();
    private GameObject holdBlock;

    public bool holdInTurn = false;
    [SerializeField] private Board board;
    
    void Awake()
    {
        int firstRandomIndex = Random.Range(0, blocks.Length);
        Block firstBlock = Instantiate(blocks[firstRandomIndex], transform.position, Quaternion.identity).GetComponent<Block>();
        firstBlock.gameObject.tag = "OnBoard";
        Debug.Log("tag: " + firstBlock.gameObject.tag);
        // board.SetOnBoardBlock(ref firstBlock);
        
        // Spawn block in next area
        while(nextBlocks.Count < 5)
        {
            int randomIndex = Random.Range(0, blocks.Length);
            GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[nextBlocks.Count].position, Quaternion.identity);
            if(newBlock.TryGetComponent<Block>(out Block block))
            {
                block.RenderCenter(nextAreas[nextBlocks.Count].position);
                block.enabled = false;
            }
            nextBlocks.Add(newBlock);
        }
    }

    public void Spawn()
    {
        var oldPosition = nextBlocks[0].transform.position;
        nextBlocks[0].transform.position = transform.position;
        if( nextBlocks[0].TryGetComponent(out Block nextBlock))
        {
            // board.ValidMovement(nextBlock, Vector3.down);
            if(board.ValidMovement(nextBlock, Vector3.down)){
                nextBlock.enabled = true;
                board.SetOnBoardBlock(ref nextBlock);
            } else
            {
                nextBlocks[0].transform.position = oldPosition;
                GameManager.instance.GameOver();
                return;
            }
        }
        nextBlocks.RemoveAt(0);
        UpdateNextBlocks();
        holdInTurn = false;
    }

    private void LoadNextBlock()
    {
        
    }

    public void Hold(GameObject blockGo)
    {
        if(!holdInTurn)
        {
            blockGo.transform.rotation = Quaternion.identity;
            if(blockGo.TryGetComponent<Block>(out Block block))
            {
                block.RenderCenter(holdArea.position);
                block.enabled = false;
            }

            if(holdBlock == null)
            {
                holdBlock = blockGo;
                Spawn();
            } else {
                holdBlock.TryGetComponent<Block>(out Block tempBlock);
                {
                    tempBlock.enabled = true;
                }
                holdBlock.transform.position = transform.position;
                holdBlock = blockGo;
            }
            holdInTurn = true;
        } else return;
    }

    private void UpdateNextBlocks()
    {
        // change position block in next area
        for (int i = 0; i < nextBlocks.Count; i++)
        {
            if(nextBlocks[i].TryGetComponent(out Block nextBlock))
            {
                nextBlock.RenderCenter(nextAreas[i].position);
            }
        }

        // spawn new block to next area
        int randomIndex = Random.Range(0, blocks.Length);
        GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[4].position, Quaternion.identity);


        if(newBlock.TryGetComponent(out Block block))
        {
            block.RenderCenter(nextAreas[4].position);
            block.enabled = false;
        }
        nextBlocks.Add(newBlock);
    }

}
