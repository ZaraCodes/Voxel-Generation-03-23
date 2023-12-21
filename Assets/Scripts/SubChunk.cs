using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubChunk : MonoBehaviour
{
    /// <summary>
    /// Reference to the chunk this sub chunk belongs to
    /// </summary>
    public Chunk Chunk { get; set; }

    public void RemoveBlockAt(Vector3Int blockPos)
    {
        Vector2Int chunkPos = Chunk.ChunkPos;

        int level = CalculateLocalBlockPosition(ref blockPos);

        Chunk.UpdateBlock(EBlockType.Air, level, blockPos.x, blockPos.y, blockPos.z);

        Chunk[] neighborChunks = new Chunk[]
        {
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
        };

        List<BlockAndItsFaces> blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level, blockAndItsFaces, false);

        if (blockPos.y == 0 && level != 0)
            ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level - 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level - 1, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight), false);
        else if (blockPos.y == ChunkManager.Instance.Width - 1 && level != ChunkManager.Instance.chunkHeight - 1)
            ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level + 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level + 1, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight), false);

        if (blockPos.x == 0)
            UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
        else if (blockPos.x == ChunkManager.Instance.Width - 1)
            UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
        if (blockPos.z == 0)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
        else if (blockPos.z == ChunkManager.Instance.Width - 1)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);

    }

    public static int CalculateLocalBlockPosition(ref Vector3Int blockPos)
    {
        int level = (blockPos.y / ChunkManager.Instance.Width) + ChunkManager.Instance.chunkOffsetY;

        blockPos = new(blockPos.x % ChunkManager.Instance.Width, blockPos.y % ChunkManager.Instance.Width, blockPos.z % ChunkManager.Instance.Width);
        blockPos += new Vector3Int(ChunkManager.Instance.Width / 2, 0, ChunkManager.Instance.Width / 2);
        if (blockPos.x < 0) blockPos.x += ChunkManager.Instance.Width;
        if (blockPos.z < 0) blockPos.z += ChunkManager.Instance.Width;
        if (blockPos.y < 0)
        {
            blockPos.y += ChunkManager.Instance.Width;
            level -= 1;
        }
        blockPos = new(blockPos.x % ChunkManager.Instance.Width, blockPos.y % ChunkManager.Instance.Width, blockPos.z % ChunkManager.Instance.Width);
        return level;
    }

    public EBlockType? GetBlock(Vector3Int blockPos)
    {
        int level = CalculateLocalBlockPosition(ref blockPos);
        return Chunk.GetBlock(level, blockPos.x, blockPos.y, blockPos.z);
    }

    private static void UpdateNeighborChunk(Vector2Int neighborChunkPos, int level, Chunk[] neighborChunks, int index)
    {
        List<BlockAndItsFaces> blockAndItsFaces;
        Chunk neighborChunk = neighborChunks[index];
        Chunk[] newNeighborChunks = new Chunk[]
        {
            ChunkManager.Instance.GetChunk(new Vector2Int(neighborChunkPos.x + 1, neighborChunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(neighborChunkPos.x - 1, neighborChunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(neighborChunkPos.x, neighborChunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(neighborChunkPos.x, neighborChunkPos.y - 1)).GetComponent<Chunk>()
        };
        blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(neighborChunk, newNeighborChunks, level, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(neighborChunk, level, blockAndItsFaces, false);
    }

    public static void AddBlockAt(Vector3Int blockPos, EBlockType blockType, Chunk chunk)
    {
        Vector2Int chunkPos = chunk.ChunkPos;

        int level = CalculateLocalBlockPosition(ref blockPos);

        chunk.UpdateBlock(blockType, level, blockPos.x, blockPos.y, blockPos.z);

        Chunk[] neighborChunks = new Chunk[]
        {
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
        };

        List<BlockAndItsFaces> blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level, blockAndItsFaces, false);

        if (blockPos.y == 0 && level != 0)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level - 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level - 1, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight), false);
        else if (blockPos.y == ChunkManager.Instance.Width - 1 && level != ChunkManager.Instance.chunkHeight - 1)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level + 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level + 1, ChunkManager.Instance.Width, ChunkManager.Instance.chunkHeight), false);

        if (blockPos.x == 0)
            UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
        else if (blockPos.x == ChunkManager.Instance.Width - 1)
            UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
        if (blockPos.z == 0)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
        else if (blockPos.z == ChunkManager.Instance.Width - 1)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);
    }
}
