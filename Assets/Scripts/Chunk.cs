using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Block[,,,] subChunks;

    public Vector2Int ChunkPos { get; set; }

    public void UpdateBlock(BlockType blockType, int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.width && y >= 0 && y < ChunkManager.Instance.width && z >= 0 && z < ChunkManager.Instance.width)
        {
            Debug.Log($"{subChunks[level, x, y, z].position} {subChunks[level, x, y, z].type}");
            subChunks[level, x, y, z].type = blockType;
        }
    }
}
