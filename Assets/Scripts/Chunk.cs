using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Chunk : MonoBehaviour
{
    public Block[,,,] subChunks;
    public bool generationFinished;

    public Vector2Int ChunkPos { get; set; }

    public void UpdateBlock(EBlockType blockType, int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.width && y >= 0 && y < ChunkManager.Instance.width && z >= 0 && z < ChunkManager.Instance.width)
        {
            Debug.Log($"{subChunks[level, x, y, z].Position} {subChunks[level, x, y, z].Type}");
            subChunks[level, x, y, z].Type = blockType;
        }
    }

    public Block GetBlock(int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.width && y >= 0 && y < ChunkManager.Instance.width && z >= 0 && z < ChunkManager.Instance.width)
        {
            return subChunks[level, x, y, z];
        }
        return null;
    }
}
