using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
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

    /// <summary>
    /// This method generates the face data for the chunk during chunk generation
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="neighborChunks"></param>
    /// <param name="chunkData"></param>
    /// <param name="level"></param>
    /// <param name="size"></param>
    /// <param name="height"></param>
    /// <param name="chunkGeneration"></param>
    public void BuildBlockSides(Chunk chunk, Chunk[] neighborChunks, List<BlockAndItsFaces>[] chunkData, int level, int size, int height, bool chunkGeneration = true)
    {
        List<BlockAndItsFaces> subChunkData = new();
        var blockRegistry = GameManager.Instance.BlockRegistry;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    // calculate block position
                    int blockY = ChunkManager.Instance.GetBlockY(level, j);
                    Vector3 blockPosition = new(chunk.GetBlockX(i), blockY, chunk.GetBlockZ(k));
                    var block = chunk.subChunks[level, i, j, k];
                    if (blockRegistry.GetRenderType(block) == EBlockRenderType.Air) continue;
                    List<EBlockFace> faces = new();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (blockY + blockSide.Item2 < -ChunkManager.Instance.chunkOffsetY * size)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (blockY + blockSide.Item2 >= size * (height - ChunkManager.Instance.chunkOffsetY))
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = (blockY + blockSide.Item2) / size + ChunkManager.Instance.chunkOffsetY;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            var neighborBlock = chunk.subChunks[level, i + blockSide.Item1, j + blockSide.Item2, k + blockSide.Item3];
                            if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                        }
                        // only execute this if this method gets called during chunk generation
                        else if (chunkGeneration)
                        {
                            if (i + blockSide.Item1 == size)
                            {
                                var neighborBlock = neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (i + blockSide.Item1 == -1)
                            {
                                var neighborBlock = neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (j + blockSide.Item2 == size)
                            {
                                var neighborBlock = chunk.GetBlock(level + 1, i + blockSide.Item1, 0, k + blockSide.Item3);
                                if (neighborBlock != null && CheckBlockBorder(block, (EBlockType) neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (j + blockSide.Item2 == -1)
                            {
                                var neighborBlock = chunk.GetBlock(level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3);
                                if (neighborBlock != null && CheckBlockBorder(block, (EBlockType) neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (k + blockSide.Item3 == size)
                            {
                                var neighborBlock = neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                            else if (k + blockSide.Item3 == -1)
                            {
                                var neighborBlock = neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = blockPosition,
                        blockFaces = faces.ToArray(),
                        blockType = block,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        if (chunkGeneration) chunkData[level] = subChunkData;
        else chunkData[0] = subChunkData;
    }

    private static bool CheckBlockBorder(EBlockType block, EBlockType neighborBlock)
    {
        var br = GameManager.Instance.BlockRegistry;
        return br.GetRenderType(block) == EBlockRenderType.Solid && (br.GetRenderType(neighborBlock) != EBlockRenderType.Solid) ||
            br.GetRenderType(block) == EBlockRenderType.Water && br.GetRenderType(neighborBlock) == EBlockRenderType.Air ||
            br.GetRenderType(block) == EBlockRenderType.Water && ((br.GetRenderType(neighborBlock) == EBlockRenderType.Water && block != neighborBlock) || br.GetRenderType(neighborBlock) == EBlockRenderType.Transparent) ||
            br.GetRenderType(block) == EBlockRenderType.Transparent;
    }

    /// <summary>
    /// This is for recalculating the block sides when setting or removing a block
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="neighborChunks"></param>
    /// <param name="level"></param>
    /// <param name="size"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<BlockAndItsFaces> BuildBlockSides(Chunk chunk, Chunk[] neighborChunks, int level, int size, int height)
    {        
        List<BlockAndItsFaces> subChunkData = new List<BlockAndItsFaces>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    var block = chunk.subChunks[level, i, j, k];
                    var blockY = ChunkManager.Instance.GetBlockY(level, j);
                    Vector3 blockPosition = new(chunk.GetBlockX(i), blockY, chunk.GetBlockZ(k));


                    //if (block.Type == BlockType.Air) continue;
                    if (GameManager.Instance.BlockRegistry.GetRenderType(block) == EBlockRenderType.Air) continue;
                    List<EBlockFace> faces = new();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (blockY + blockSide.Item2 < -ChunkManager.Instance.chunkOffsetY * size)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (blockY + blockSide.Item2 >= size * (height - ChunkManager.Instance.chunkOffsetY))
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = (blockY + blockSide.Item2) / size + ChunkManager.Instance.chunkOffsetY;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            var neighborBlock = chunk.subChunks[level, i + blockSide.Item1, j + blockSide.Item2, k + blockSide.Item3];
                            if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                        }
                        else
                        {
                            if (j + blockSide.Item2 == -1 && level != 0)
                            {
                                var neighborBlock = chunk.subChunks[level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (chunk.subChunks[level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3].Type == BlockType.Air)
                                //    {
                                //        AddBlockFace(faces, blockSide);
                                //    }
                            }
                            else if (j + blockSide.Item2 == size && level != height)
                            {
                                var neighborBlock = chunk.subChunks[level + 1, i + blockSide.Item1, 0, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (chunk.subChunks[level + 1, i + blockSide.Item1, 0, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            // 0: x+  1: x-  2: z+  3: z-
                            if (i + blockSide.Item1 == size)
                            {
                                var neighborBlock = neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (i + blockSide.Item1 == -1)
                            {
                                var neighborBlock = neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (k + blockSide.Item3 == size)
                            {
                                var neighborBlock = neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                            else if (k + blockSide.Item3 == -1)
                            {
                                var neighborBlock = neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1];
                                if (CheckBlockBorder(block, neighborBlock)) AddBlockFace(faces, blockSide);
                                //if (neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1].Type == BlockType.Air)
                                //{
                                //    AddBlockFace(faces, blockSide);
                                //}
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = blockPosition,
                        blockFaces = faces.ToArray(),
                        blockType = block,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        return subChunkData;
    }

    public Task StartGenerateBlockData(Chunk chunk, int level, int size, AnimationCurve worldHeightCurve)
    {
        return Task.Run(() => { GenerateBlockData(chunk, level, size, worldHeightCurve); });
    }

    private void GenerateBlockData(Chunk chunk, int level, int size, AnimationCurve worldHeightCurve)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    EBlockType b;
                    Vector3 blockPosition = new(chunk.GetBlockX(x), ChunkManager.Instance.GetBlockY(level, y), chunk.GetBlockZ(z));
                    if (Evaluate3DNoise(blockPosition, worldHeightCurve)) b = EBlockType.Stone;
                    else
                    {
                        if (blockPosition.y <= 0)
                        {
                            b = EBlockType.Water;
                        }
                        else
                        {
                            b = EBlockType.Air;
                        }
                    }
                    chunk.subChunks[level, x, y, z] = b;
                }
            }
        }
    }

    private static void AddBlockFace(List<EBlockFace> faces, (int, int, int) blockSide)
    {
        switch (blockSide)
        {
            case (-1, 0, 0):
                faces.Add(EBlockFace.Xdown);
                break;
            case (1, 0, 0):
                faces.Add(EBlockFace.Xup);
                break;
            case (0, -1, 0):
                faces.Add(EBlockFace.Ydown);
                break;
            case (0, 1, 0):
                faces.Add(EBlockFace.Yup);
                break;
            case (0, 0, -1):
                faces.Add(EBlockFace.Zdown);
                break;
            case (0, 0, 1):
                faces.Add(EBlockFace.Zup);
                break;
            default:
                break;
        }
    }

    public bool Evaluate3DNoise(Vector3 position, AnimationCurve worldHeightCurve)
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

        var threshold = EvaluateHilliness(position) * 75f;
        var worldHeight = EvaluateWorldHeight(position, worldHeightCurve);

        if (-worldHeight + 0.1f + position.y / threshold < noise)
            //if ((worldHeight - 0.2f) * 1.5f > position.y / 100f)
            return true;
        return false;
    }

    /// <summary>Evaluates the hilliness value at the given position</summary>
    /// <param name="position">A value between 0 and 1</param>
    /// <returns></returns>
    public float EvaluateHilliness(Vector3 position)
    {
        return (hillinessNoise.Evaluate(new Vector3(position.x * frequency, 100f, position.z * frequency) / 10f) + 1f) / 2f;
    }

    /// <summary>Evaluates the world height at the given position</summary>
    /// <param name="position">A value between 0 and 1</param>
    /// <returns></returns>
    public float EvaluateWorldHeight(Vector3 position, AnimationCurve worldHeightCurve)
    {
        return worldHeightCurve.Evaluate((worldHeightNoise.Evaluate(new Vector3(position.x * frequency, 0f, position.z * frequency) / 20f) + 1f) / 2f);
        //return (worldHeightNoise.Evaluate(new Vector3(position.x * frequency, 0f, position.z * frequency) / 20f) + 1f) / 2f;
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
                    if (level == 0 && y < 5)  // bedrock pattern
                    {
                        float val = baseNoise.Evaluate(new Vector3(chunk.Position.x + x, y * 100, chunk.Position.z + z));
                        if (((val + 1f) / 2f) * ((val + 1f) / 2f) > y / 5f)
                        {
                            chunk.subChunks[level, x, y, z] = EBlockType.Bedrock;
                            continue;
                        }
                    }
                    var block = chunk.subChunks[level, x, y, z];
                    if (block == EBlockType.Stone)
                    {
                        // If the block above this block is air, then this block is grass
                        AirCheckResult result = AirCheck(chunk, x, y, z, level, height, size, 1);
                        if (result == AirCheckResult.Break)
                        {
                            chunk.subChunks[level, x, y, z] = EBlockType.Grass;
                            break;
                        }
                        else if (result == AirCheckResult.Continue)
                        {
                            chunk.subChunks[level, x, y, z] = EBlockType.Grass;
                            if (x == size / 2 && z == size / 2 && chunk.subChunks[level, x, y, z] == EBlockType.Grass) GenerateTree(chunk, level, size, x, y, z);
                            continue;
                        }

                        // If the block two or three blocks above is air, this block will be dirt
                        result = AirCheck(chunk, x, y, z, level, height, size, 2);
                        if (result == AirCheckResult.Break) break;
                        else if (result == AirCheckResult.Continue)
                        {
                            chunk.subChunks[level, x, y, z] = EBlockType.Dirt;
                            continue;
                        }

                        result = AirCheck(chunk, x, y, z, level, height, size, 3);
                        if (result == AirCheckResult.Break) break;
                        else if (result == AirCheckResult.Continue)
                        {
                            chunk.subChunks[level, x, y, z] = EBlockType.Dirt;
                            continue;
                        }
                    }
                }
            }
        }
    }

    public void GenerateTree(Chunk chunk, int level, int size, int x, int y, int z)
    {
        // replace grass block with dirt
        //if (y == 0) chunk.UpdateBlock(EBlockType.Dirt, level - 1, x, size - 1, z);
        //else
        var treeHeight = 8;
        chunk.UpdateBlock(EBlockType.Dirt, level, x, y, z);
        
        for (int i = 1; i < treeHeight; i++)
        {
            int level2 = level;
            int y2 = y;
            while (y2 + i >= size)
            {
                y2 -= size;
                level2++;
            }
            if (i > treeHeight - 3)
            {
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z - 1);
            }
            else
                chunk.UpdateBlock(EBlockType.WoodLog, level2, x, y2 + i, z);
            if (i == treeHeight - 2)
            {
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z + 1);
            }
            else if (i == treeHeight - 3 || i == treeHeight - 4)
            {
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z + 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 1, y2 + i, z - 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z + 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x, y2 + i, z - 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z + 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 1, y2 + i, z - 2);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 2, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 2, y2 + i, z - 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 2, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 2, y2 + i, z);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x + 2, y2 + i, z + 1);
                chunk.UpdateBlock(EBlockType.Leafes, level2, x - 2, y2 + i, z + 1);
            }
        }
    }

    private AirCheckResult AirCheck(Chunk chunk, int x, int y, int z, int levelIdx, int height, int size, int offset)
    {
        int ycheck = y + offset;
        if (ycheck >= size)
        {
            levelIdx++;
            ycheck -= size;
            if (levelIdx == height)
            {
                return AirCheckResult.Break;
            }
        }
        var blockToCheck = chunk.subChunks[levelIdx, x, ycheck, z];
        if (blockToCheck == EBlockType.Air)
        {
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
