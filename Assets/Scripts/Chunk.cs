using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Block[,,,] subChunks;

    public void UpdateBlock(BlockType blockType, int level, int x, int y, int z)
    {
        if (level < 8 && level >= 0 && x >= 0 && x < 16 && y >= 0 && y < 16 && z >= 0 && z < 16)
        {
            Debug.Log(subChunks[level, x, y, z].type.ToString());
            subChunks[level, x, y, z].type = blockType;
        }
    }
}
