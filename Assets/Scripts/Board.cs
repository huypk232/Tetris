using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private const int LeftLimit = 0;
    private const int RightLimit = 10;
    private const int BottomLimit = 0;
    private const int TopLimit = 23;
    // public Transform[][] Tiles = new Transform[RightLimit - LeftLimit][]; // share among block instances
    //
    // private void Awake()
    // {
    //     for (int column = 0; column < TopLimit - BottomLimit; column++)
    //     {
    //         Tiles[column] = new Transform[TopLimit - BottomLimit];
    //     }
    // }
    
}
