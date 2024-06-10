using System;
using UnityEngine;

[Serializable]
public class Chunk : MonoBehaviour
{
    public EBlockType[,,,] SubChunks;
    public bool GenerationFinished;

    public Vector2Int ChunkPos { get; set; }

    public Transform Transform { get; set; }

    public Vector3 Position { get; set; }

    public bool[] QueuedMeshUpdates;

    public void SetBlock(EBlockType blockType, int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.Width && y >= 0 && y < ChunkManager.Instance.Width && z >= 0 && z < ChunkManager.Instance.Width)
        {
            SubChunks[level, x, y, z] = blockType;
        }
    }

    /// <summary>
    /// This method is used to set blocks in neighboring chunks in case a generated structure extends into that
    /// </summary>
    /// <param name="blockType"></param>
    /// <param name="typeToReplace"></param>
    /// <param name="neighbors"></param>
    /// <param name="level"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public bool SetBlock(EBlockType blockType, EBlockType typeToReplace, Chunk[] neighbors, int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && y >= 0 && y < ChunkManager.Instance.Width)
        {            
            if (x >= ChunkManager.Instance.Width)
            {
                if (z >= ChunkManager.Instance.Width)
                    return SetBlock(neighbors[4], blockType, typeToReplace, level, x - ChunkManager.Instance.Width, y, z - ChunkManager.Instance.Width);
                else if (z < 0)
                    return SetBlock(neighbors[5], blockType, typeToReplace, level, x - ChunkManager.Instance.Width, y, z + ChunkManager.Instance.Width);
                else
                    return SetBlock(neighbors[0], blockType, typeToReplace, level, x - ChunkManager.Instance.Width, y, z);
            }
            else if (x < 0)
            {
                if (z >= ChunkManager.Instance.Width)
                    return SetBlock(neighbors[6], blockType, typeToReplace, level, x + ChunkManager.Instance.Width, y, z - ChunkManager.Instance.Width);
                else if (z < 0)
                    return SetBlock(neighbors[7], blockType, typeToReplace, level, x + ChunkManager.Instance.Width, y, z + ChunkManager.Instance.Width);
                else
                    return SetBlock(neighbors[1], blockType, typeToReplace, level, x + ChunkManager.Instance.Width, y, z);
            }
            else
            {
                if (z >= ChunkManager.Instance.Width)
                    return SetBlock(neighbors[2], blockType, typeToReplace, level, x, y, z - ChunkManager.Instance.Width);
                else if (z < 0)
                    return SetBlock(neighbors[3], blockType, typeToReplace, level, x, y, z + ChunkManager.Instance.Width);
                else
                    return SetBlock(this, blockType, typeToReplace, level, x, y, z);
            }
        }
        return false;
    }

    private static bool SetBlock(Chunk chunk, EBlockType blockType, EBlockType typeToReplace, int level, int x, int y, int z)
    {
        if (chunk.GenerationFinished)
            chunk.QueuedMeshUpdates[level] = true;
        
        if (chunk.SubChunks[level, x, y, z] == typeToReplace)
        {
            chunk.SubChunks[level, x, y, z] = blockType;
            return true;
        }
        return false;
    }

    public EBlockType? GetBlock(int level, int x, int y, int z)
    {
        if (level < ChunkManager.Instance.chunkHeight && level >= 0 && x >= 0 && x < ChunkManager.Instance.Width && y >= 0 && y < ChunkManager.Instance.Width && z >= 0 && z < ChunkManager.Instance.Width)
        {
            return SubChunks[level, x, y, z];
        }
        return null;
    }

    public int GetBlockX(int x) => (int)(Position.x - ChunkManager.Instance.Width / 2 + x);

    public int GetBlockZ(int z) => (int)(Position.z - ChunkManager.Instance.Width / 2 + z);
}
