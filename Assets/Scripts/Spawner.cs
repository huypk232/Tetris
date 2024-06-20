using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Tooltip("I_Block, J_Block, L_Block, O_Block, S_Block, T_Block, Z_Block")]
    public GameObject[] blocks;
    public Transform[] nextAreas;
    public Transform holdArea;

    private const int TotalBlocks = 7;
    
    private readonly List<GameObject> _nextBlocks = new();
    private GameObject _holdBlock;
    private Shadow _shadow;
    private readonly int[] _spawnCounter = new int[TotalBlocks];
    private int _remainBlockInCycle;

    
    [SerializeField] private Board board;
    
    
    private void Awake()
    {
        _shadow = FindObjectOfType<Shadow>();
    }

    private void Start()
    {
        InitSpawnCounter();
        
        int firstRandomIndex = Random.Range(0, blocks.Length);
        _spawnCounter[firstRandomIndex]--;
        _remainBlockInCycle--;
        
        Block firstBlock = Instantiate(blocks[firstRandomIndex], transform.position, Quaternion.identity).GetComponent<Block>();
        firstBlock.SetBoard(board);
        _shadow.Follow(firstBlock, transform.position);
        
        while(_nextBlocks.Count < 5)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, blocks.Length);
            } while (_spawnCounter[randomIndex] == 0);

            _spawnCounter[randomIndex]--;
            _remainBlockInCycle--;
            GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[_nextBlocks.Count].position, Quaternion.identity);
            if(newBlock.TryGetComponent(out Block block))
            {
                block.MoveTo(nextAreas[_nextBlocks.Count].position);
                block.enabled = false;
            }
            _nextBlocks.Add(newBlock);
        }
    }

    private void InitSpawnCounter()
    {
        for (int i = 0; i < _spawnCounter.Length; i++)
        {
            var randomQuantity = Random.Range(1, 3);
            _spawnCounter[i] = randomQuantity;
            _remainBlockInCycle += randomQuantity;
        }
              
    }

    public void Spawn()
    {
        var oldPosition = _nextBlocks[0].transform.position;
        _nextBlocks[0].transform.position = transform.position;
        if( _nextBlocks[0].TryGetComponent(out Block nextBlock))
        {
            if(board.ValidMovement(nextBlock)){
                nextBlock.enabled = true;
                nextBlock.SetBoard(board);
                _shadow.Follow(nextBlock, transform.position);
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

        if (_remainBlockInCycle <= 0)
            InitSpawnCounter();
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, blocks.Length);
        } while (_spawnCounter[randomIndex] == 0);

        _spawnCounter[randomIndex]--;
        _remainBlockInCycle--;
        
        // spawn new block to next area
        GameObject newBlock = Instantiate(blocks[randomIndex], nextAreas[4].position, Quaternion.identity);


        if(newBlock.TryGetComponent(out Block block))
        {
            block.MoveTo(nextAreas[4].position);
            block.enabled = false;
        }
        _nextBlocks.Add(newBlock);
    }

}
