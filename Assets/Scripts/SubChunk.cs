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

        Chunk.UpdateBlock(BlockType.Air, level, blockPos.x, blockPos.y, blockPos.z);

        Chunk[] neighborChunks = new Chunk[]
        {
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
        };

        List<BlockAndItsFaces> blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level, blockAndItsFaces, false);

        if (blockPos.y == 0 && level != 0)
            ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level - 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level - 1, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight), false);
        else if (blockPos.y == ChunkManager.Instance.width - 1 && level != ChunkManager.Instance.chunkHeight - 1)
            ChunkManager.Instance.Generator.GenerateSubChunk(Chunk, level + 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(Chunk, neighborChunks, level + 1, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight), false);

        if (blockPos.x == 0)
            UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
        else if (blockPos.x == ChunkManager.Instance.width - 1)
            UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
        if (blockPos.z == 0)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
        else if (blockPos.z == ChunkManager.Instance.width - 1)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);

    }

    public static int CalculateLocalBlockPosition(ref Vector3Int blockPos)
    {
        int level = (blockPos.y / ChunkManager.Instance.width) + ChunkManager.Instance.chunkOffsetY;

        blockPos = new(blockPos.x % ChunkManager.Instance.width, blockPos.y % ChunkManager.Instance.width, blockPos.z % ChunkManager.Instance.width);
        blockPos += new Vector3Int(ChunkManager.Instance.width / 2, 0, ChunkManager.Instance.width / 2);
        if (blockPos.x < 0) blockPos.x += ChunkManager.Instance.width;
        if (blockPos.z < 0) blockPos.z += ChunkManager.Instance.width;
        if (blockPos.y < 0)
        {
            blockPos.y += ChunkManager.Instance.width;
            level -= 1;
        }
        blockPos = new(blockPos.x % ChunkManager.Instance.width, blockPos.y % ChunkManager.Instance.width, blockPos.z % ChunkManager.Instance.width);
        return level;
    }

    public Block GetBlock(Vector3Int blockPos)
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
        blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(neighborChunk, newNeighborChunks, level, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(neighborChunk, level, blockAndItsFaces, false);
    }

    public static void AddBlockAt(Vector3Int blockPos, BlockType blockType, Chunk chunk)
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

        List<BlockAndItsFaces> blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level, blockAndItsFaces, false);

        if (blockPos.y == 0 && level != 0)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level - 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level - 1, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight), false);
        else if (blockPos.y == ChunkManager.Instance.width - 1 && level != ChunkManager.Instance.chunkHeight - 1)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level + 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level + 1, ChunkManager.Instance.width, ChunkManager.Instance.chunkHeight), false);

        if (blockPos.x == 0)
            UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
        else if (blockPos.x == ChunkManager.Instance.width - 1)
            UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
        if (blockPos.z == 0)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
        else if (blockPos.z == ChunkManager.Instance.width - 1)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);
    }
}
