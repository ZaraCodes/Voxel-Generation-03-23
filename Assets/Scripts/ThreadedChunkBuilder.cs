using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadedChunkBuilder
{
    int octaves = 3;
    float frequency = 0.0625f / 2f;
    float persistence = 0.5f;
    float lacunarity = 2f;

    private int seed;
    public int Seed { get { return seed; } set { SetSeed(value); } }

    private SimplexNoise baseNoise;
    private SimplexNoise hillinessNoise;
    private SimplexNoise worldHeightNoise;

    private AnimationCurve worldHeightCurve;

    private readonly (int, int, int)[] blockSides = new (int, int, int)[] { (-1, 0, 0), (1, 0, 0), (0, -1, 0), (0, 1, 0), (0, 0, -1), (0, 0, 1) };

    public ThreadedChunkBuilder(AnimationCurve worldHeightCurve)
    {
        this.worldHeightCurve = worldHeightCurve;
        Seed = Random.Range(int.MinValue, int.MaxValue);
        GameManager.Instance.ChunkBuilder = this;
    }

    private void SetSeed(int seed)
    {
        this.seed = seed;
        baseNoise = new SimplexNoise(seed);
        hillinessNoise = new SimplexNoise(++seed);
        worldHeightNoise = new SimplexNoise(++seed);
    }

    public Task StartBuildBlockSides(Chunk chunk, Chunk[] neighborChunks, List<BlockAndItsFaces>[] chunkData, int level, int size, int height)
    {
        return Task.Run(() => { BuildBlockSides(chunk, neighborChunks, chunkData, level, size, height); });
    }

    public void BuildBlockSides(Chunk chunk, Chunk[] neighborChunks, List<BlockAndItsFaces>[] chunkData, int level, int size, int height, bool chunkGeneration = true)
    {
        List<BlockAndItsFaces> subChunkData = new();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    Block block = chunk.subChunks[level, i, j, k];
                    if (block.RenderType == BlockRenderType.Air) continue;
                    List<BlockFace> faces = new();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (block.position.y + blockSide.Item2 < -ChunkManager.Instance.chunkOffsetY * size)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (block.position.y + blockSide.Item2 >= size * (height - ChunkManager.Instance.chunkOffsetY))
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = ((int)block.position.y + blockSide.Item2) / size + ChunkManager.Instance.chunkOffsetY;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            Block neighborBlock = chunk.subChunks[level, i + blockSide.Item1, j + blockSide.Item2, k + blockSide.Item3];
                            if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                        }
                        // only execute this if this method gets called during chunk generation
                        else if (chunkGeneration)
                        {
                            if (i + blockSide.Item1 == size)
                            {
                                Block neighborBlock = neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (i + blockSide.Item1 == -1)
                            {
                                Block neighborBlock = neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (j + blockSide.Item2 == size)
                            {
                                Block neighborBlock = chunk.GetBlock(level + 1, i + blockSide.Item1, 0, k + blockSide.Item3);
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (j + blockSide.Item2 == -1)
                            {
                                Block neighborBlock = chunk.GetBlock(level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3);
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (k + blockSide.Item3 == size)
                            {
                                Block neighborBlock = neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (k + blockSide.Item3 == -1)
                            {
                                Block neighborBlock = neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = block.position,
                        blockFaces = faces.ToArray(),
                        blockType = block.Type,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        if (chunkGeneration) chunkData[level] = subChunkData;
        else chunkData[0] = subChunkData;
    }

    private static bool CheckBlockBorder(Block block, Block neighborBlock)
    {
        if (neighborBlock == null) return false;
        return block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
            (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
            (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type);
    }

    public List<BlockAndItsFaces> BuildBlockSides(Chunk chunk, Chunk[] neighborChunks, int level, int size, int height)
    {
        List<BlockAndItsFaces> subChunkData = new List<BlockAndItsFaces>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    Block block = chunk.subChunks[level, i, j, k];
                    //if (block.Type == BlockType.Air) continue;
                    if (block.RenderType == BlockRenderType.Air) continue;
                    List<BlockFace> faces = new List<BlockFace>();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (block.position.y + blockSide.Item2 < -ChunkManager.Instance.chunkOffsetY * size)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (block.position.y + blockSide.Item2 >= size * (height - ChunkManager.Instance.chunkOffsetY))
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = ((int)block.position.y + blockSide.Item2) / size + ChunkManager.Instance.chunkOffsetY;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            Block neighborBlock = chunk.subChunks[level, i + blockSide.Item1, j + blockSide.Item2, k + blockSide.Item3];
                            if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                            {
                                AddBlockFace(faces, blockSide);
                            }
                        }
                        else
                        {
                            if (j + blockSide.Item2 == -1 && level != 0)
                            {
                                Block neighborBlock = chunk.subChunks[level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (chunk.subChunks[level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3].Type == BlockType.Air)
                                //    {
                                //        AddBlockFace(faces, blockSide);
                                //    }
                            }
                            else if (j + blockSide.Item2 == size && level != height)
                            {
                                Block neighborBlock = chunk.subChunks[level + 1, i + blockSide.Item1, 0, k + blockSide.Item3];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (chunk.subChunks[level + 1, i + blockSide.Item1, 0, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            // 0: x+  1: x-  2: z+  3: z-
                            if (i + blockSide.Item1 == size)
                            {
                                Block neighborBlock = neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (i + blockSide.Item1 == -1)
                            {
                                Block neighborBlock = neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (k + blockSide.Item3 == size)
                            {
                                Block neighborBlock = neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (k + blockSide.Item3 == -1)
                            {
                                Block neighborBlock = neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1];
                                if (block.RenderType == BlockRenderType.Solid && (neighborBlock.RenderType == BlockRenderType.Transparent || neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Air) ||
                                    (block.RenderType == BlockRenderType.Transparent && neighborBlock.RenderType == BlockRenderType.Transparent && block.Type != neighborBlock.Type))
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                                //if (neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = block.position,
                        blockFaces = faces.ToArray(),
                        blockType = block.Type,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        return subChunkData;
    }

    public Task StartGenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size, int yOffset)
    {
        return Task.Run(() => { GenerateBlockData(chunk, rootPos, cornerPos, level, size, yOffset); });
    }

    private void GenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size, int yOffset)
    {
        // (int)(rootPos.x + cornerPos.x + size)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Block b = new();
                    // Debug.Log($"{level} {x} {y} {z}");
                    b.position = new Vector3((int)(rootPos.x + cornerPos.x) + x, (level - yOffset) * size + y, (int)(rootPos.z + cornerPos.z) + z);
                    if (Evaluate3DNoise(b.position)) b.Type = BlockType.Stone;
                    else
                    {
                        if (b.position.y <= 0)
                        {
                            b.RenderType = BlockRenderType.Transparent;
                            b.Type = BlockType.Water;
                        }
                        else
                        {
                            b.Type = BlockType.Air;
                            b.RenderType = BlockRenderType.Air;
                        }
                    }
                    chunk.subChunks[level, x, y, z] = b;
                }
            }
        }
    }

    private static void AddBlockFace(List<BlockFace> faces, (int, int, int) blockSide)
    {
        switch (blockSide)
        {
            case (-1, 0, 0):
                faces.Add(BlockFace.Xdown);
                break;
            case (1, 0, 0):
                faces.Add(BlockFace.Xup);
                break;
            case (0, -1, 0):
                faces.Add(BlockFace.Ydown);
                break;
            case (0, 1, 0):
                faces.Add(BlockFace.Yup);
                break;
            case (0, 0, -1):
                faces.Add(BlockFace.Zdown);
                break;
            case (0, 0, 1):
                faces.Add(BlockFace.Zup);
                break;
            default:
                break;
        }
    }

    public bool Evaluate3DNoise(Vector3 position)
    {
        float noise = 0f;
        float divisor = 0f;
        Vector3 octaveOffset = new(25, -4, 15);
        for (int octave = 0; octave < octaves; octave++)
        {
            noise += baseNoise.Evaluate(frequency * Mathf.Pow(lacunarity, octave) * position / 3 + octaveOffset * octave) * Mathf.Pow(persistence, octave);
            divisor += Mathf.Pow(persistence, octave);
        }
        noise /= divisor;

        var threshold = EvaluateHilliness(position) * 75;
        var worldHeight = EvaluateWorldHeight(position);

        if (-worldHeight + 0.1f + position.y / threshold < noise)
            return true;
        return false;
    }

    /// <summary>Evaluates the hilliness value at the given position</summary>
    /// <param name="position">A value between 0 and 1</param>
    /// <returns></returns>
    public float EvaluateHilliness(Vector3 position)
    {
        return (hillinessNoise.Evaluate(new Vector3(position.x * frequency, 100f, position.z * frequency) / 10) + 1) / 2;
    }

    /// <summary>Evaluates the world height at the given position</summary>
    /// <param name="position">A value between 0 and 1</param>
    /// <returns></returns>
    public float EvaluateWorldHeight(Vector3 position)
    {
        return worldHeightCurve.Evaluate((worldHeightNoise.Evaluate(new Vector3(position.x * frequency, 0f, position.z * frequency) / 20) + 1) / 2);
    }

    public Task StartPopulateChunk(Chunk chunk, int level, int size, int height)
    {
        return Task.Run(() => PopulateChunk(chunk, level, size, height));
    }

    private void PopulateChunk(Chunk chunk, int level, int size, int height)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Block block = chunk.subChunks[level, x, y, z];
                    if (block.Type == BlockType.Stone)
                    {
                        // If the block above this block is air, then this block is grass
                        AirCheckResult result = AirCheck(chunk, x, y, z, level, height, size, 1, block, BlockType.Grass);
                        if (result == AirCheckResult.Break) break;
                        else if (result == AirCheckResult.Continue) continue;

                        // If the block two or three blocks above is air, this block will be dirt
                        result = AirCheck(chunk, x, y, z, level, height, size, 2, block, BlockType.Dirt);
                        if (result == AirCheckResult.Break) break;
                        else if (result == AirCheckResult.Continue) continue;

                        result = AirCheck(chunk, x, y, z, level, height, size, 3, block, BlockType.Dirt);
                        if (result == AirCheckResult.Break) break;
                        else if (result == AirCheckResult.Continue) continue;
                    }
                }
            }
        }
    }
    private AirCheckResult AirCheck(Chunk chunk, int x, int y, int z, int levelIdx, int height, int size, int offset, Block block, BlockType replacement)
    {
        int ycheck = y + offset;
        if (ycheck >= size)
        {
            levelIdx++;
            ycheck -= size;
            if (levelIdx == height) return AirCheckResult.Break;
        }
        Block blockToCheck = chunk.subChunks[levelIdx, x, ycheck, z];
        if (blockToCheck.Type == BlockType.Air)
        {
            block.Type = replacement;
            return AirCheckResult.Continue;
        }
        return AirCheckResult.Default;
    }

    private enum AirCheckResult
    {
        Break,
        Continue,
        Default
    }
}
