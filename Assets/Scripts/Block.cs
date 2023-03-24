using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public BlockType type = BlockType.Stone;
    public Vector3 position = Vector3.zero;
}

public enum BlockType
{
    Air,
    Stone,
    Dirt,
    Grass,
    WoodPlanks,
}