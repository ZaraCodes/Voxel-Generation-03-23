using System;
using UnityEngine;

[Serializable]
public class Chunk : MonoBehaviour
{
    public EBlockType[,,,] subChunks;
    public bool generationFinished;

    public Vector2Int ChunkPos { get; set; }

    public Transform Transform { get; set; }

    public Vector3 Position { get; set; }

    public void UpdateBlock(EBlockType blockType, int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.Width && y >= 0 && y < ChunkManager.Instance.Width && z >= 0 && z < ChunkManager.Instance.Width)
        {
            subChunks[level, x, y, z] = blockType;
        }
    }

    public EBlockType? GetBlock(int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.Width && y >= 0 && y < ChunkManager.Instance.Width && z >= 0 && z < ChunkManager.Instance.Width)
        {
            return subChunks[level, x, y, z];
        }
        return null;
    }

    public int GetBlockX(int x) => (int)(Position.x - ChunkManager.Instance.Width / 2 + x);

    public int GetBlockZ(int z) => (int)(Position.z - ChunkManager.Instance.Width / 2 + z);
}
