using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubChunk : MonoBehaviour
{
    /// <summary>
    /// Reference to the chunk this sub chunk belongs to
    /// </summary>
    public Chunk chunk { get; set; }

    public void UpdateSubChunk(Vector3Int blockPos)
    {
        Vector2Int chunkPos = chunk.ChunkPos;

        int level = blockPos.y / 16;

        blockPos = new(blockPos.x % 16, blockPos.y % 16, blockPos.z % 16);
        blockPos += new Vector3Int(8, 0, 8);
        if (blockPos.x < 0) blockPos.x += 16;
        if (blockPos.z < 0) blockPos.z += 16;
        blockPos = new(blockPos.x % 16, blockPos.y % 16, blockPos.z % 16);

        chunk.UpdateBlock(BlockType.Air, level, blockPos.x, blockPos.y, blockPos.z);

        Chunk[] neighborChunks = new Chunk[]
        {
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
        };

        List<BlockAndItsFaces> blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level, 16, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level, blockAndItsFaces, false);

        if (blockPos.y == 0 && level != 0)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level - 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level - 1, 16, ChunkManager.Instance.chunkHeight), false);
        else if (blockPos.y == 16 - 1 && level != 8 - 1)
            ChunkManager.Instance.Generator.GenerateSubChunk(chunk, level + 1, ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level + 1, 16, ChunkManager.Instance.chunkHeight), false);

        if (blockPos.x == 0)
            UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
        else if (blockPos.x == 16 - 1)
            UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
        if (blockPos.z == 0)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
        else if (blockPos.z == 16 - 1)
            UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);

    }

    private void UpdateNeighborChunk(Vector2Int neighborChunkPos, int level, Chunk[] neighborChunks, int index)
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
        blockAndItsFaces = ChunkManager.Instance.Generator.threadedChunkBuilder.BuildBlockSides(neighborChunk, newNeighborChunks, level, 16, ChunkManager.Instance.chunkHeight);
        ChunkManager.Instance.Generator.GenerateSubChunk(neighborChunk, level, blockAndItsFaces, false);
    }
}
