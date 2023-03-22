using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ThreadedChunkBuilder
{
    int octaves = 3;
    float frequency = 0.0625f;
    float persistence = 0.5f;
    float lacunarity = 2f;

    private int seed;
    public int Seed { get { return seed; } set { SetSeed(value); } }

    private SimplexNoise baseNoise;

    private (int, int, int)[] blockSides = new (int, int, int)[] { (-1, 0, 0), (1, 0, 0), (0, -1, 0), (0, 1, 0), (0, 0, -1), (0, 0, 1) };

    public ThreadedChunkBuilder()
    {
        Seed = Random.Range(int.MinValue, int.MaxValue);
    }

    private void SetSeed(int seed)
    {
        this.seed = seed;
        baseNoise = new SimplexNoise(seed);
    }

    public Task StartBuildBlockSides(Chunk chunk, List<BlockAndItsFaces>[] chunkData, int level, int size, int height)
    {
        return Task.Run(() => { BuildBlockSides(chunk, chunkData, level, size, height); });
    }

    public void BuildBlockSides(Chunk chunk, List<BlockAndItsFaces>[] chunkData, int level, int size, int height, bool chunkGeneration = true)
    {
        List<BlockAndItsFaces> subChunkData = new List<BlockAndItsFaces>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    Block block = chunk.subChunks[level, i, j, k];
                    if (block.type == BlockType.Air) continue;
                    List<BlockFace> faces = new List<BlockFace>();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (block.position.y + blockSide.Item2 < 0)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (block.position.y + blockSide.Item2 >= size * height)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = ((int)block.position.y + blockSide.Item2) / size;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        //if (chunk.subChunks[lookupY].TryGetValue($"{block.position.x + blockSide.Item1}/{block.position.y + blockSide.Item2}/{block.position.z + blockSide.Item3}", out Block neighborBlock))
                        //try
                        //{
                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            if (chunk.subChunks[
                                level,
                                i + blockSide.Item1,
                                j + blockSide.Item2,
                                k + blockSide.Item3
                                ].type == BlockType.Air)
                            {
                                AddBlockFace(faces, blockSide);
                            }
                        }
                        // only execute this if this method gets called during chunk generation
                        else if (chunkGeneration)
                        {
                            if (!Evaluate3DNoise(new(block.position.x + blockSide.Item1, block.position.y + blockSide.Item2, block.position.z + blockSide.Item3)))
                            {
                                AddBlockFace(faces, blockSide);
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = block.position,
                        blockFaces = faces.ToArray(),
                        blockType = block.type,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        if (chunkGeneration) chunkData[level] = subChunkData;
        else chunkData[0] = subChunkData;
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
                    if (block.type == BlockType.Air) continue;
                    List<BlockFace> faces = new List<BlockFace>();

                    foreach ((int, int, int) blockSide in blockSides)
                    {
                        if (block.position.y + blockSide.Item2 < 0)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        else if (block.position.y + blockSide.Item2 >= size * height)
                        {
                            AddBlockFace(faces, blockSide);
                            continue;
                        }
                        int lookupY = ((int)block.position.y + blockSide.Item2) / size;

                        if (lookupY < 0 || lookupY >= chunk.subChunks.Length) continue;

                        //if (chunk.subChunks[lookupY].TryGetValue($"{block.position.x + blockSide.Item1}/{block.position.y + blockSide.Item2}/{block.position.z + blockSide.Item3}", out Block neighborBlock))
                        //try
                        //{
                        if (i + blockSide.Item1 < size && j + blockSide.Item2 < size && k + blockSide.Item3 < size && i + blockSide.Item1 != -1 && j + blockSide.Item2 != -1 && k + blockSide.Item3 != -1)
                        {
                            if (chunk.subChunks[
                                level,
                                i + blockSide.Item1,
                                j + blockSide.Item2,
                                k + blockSide.Item3
                                ].type == BlockType.Air)
                            {
                                AddBlockFace(faces, blockSide);
                            }
                        }
                        // only execute this if this method gets called during chunk generation
                        else 
                        {
                            if (j + blockSide.Item2 == -1 && level != 0)
                            {
                                if (chunk.subChunks[level - 1, i + blockSide.Item1, size - 1, k + blockSide.Item3].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                            else if (j + blockSide.Item2 == size && level != height)
                            {
                                if (chunk.subChunks[level + 1, i + blockSide.Item1, 0, k + blockSide.Item3].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                            // 0: x+  1: x-  2: z+  3: z-
                            if (i + blockSide.Item1 == size)
                            {
                                if (neighborChunks[0].subChunks[level, 0, j + blockSide.Item2, k + blockSide.Item3].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                            else if (i + blockSide.Item1 == -1)
                            {
                                if (neighborChunks[1].subChunks[level, size - 1, j + blockSide.Item2, k + blockSide.Item3].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                            else if (k + blockSide.Item3 == size)
                            {
                                if (neighborChunks[2].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, 0].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                            else if (k + blockSide.Item3 == -1)
                            {
                                if (neighborChunks[3].subChunks[level, i + blockSide.Item1, j + blockSide.Item2, size - 1].type == BlockType.Air)
                                {
                                    AddBlockFace(faces, blockSide);
                                }
                            }
                        }
                    }
                    BlockAndItsFaces blockAndItsFaces = new()
                    {
                        position = block.position,
                        blockFaces = faces.ToArray(),
                        blockType = block.type,
                    };
                    subChunkData.Add(blockAndItsFaces);
                }
            }
        }
        return subChunkData;
    }

    public Task StartGenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size)
    {
        return Task.Run(() => { GenerateBlockData(chunk, rootPos, cornerPos, level, size); });
    }

    private void GenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size)
    {
        // (int)(rootPos.x + cornerPos.x + size)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Block b = new();
                    b.position = new Vector3((int)(rootPos.x + cornerPos.x) + x, level * size + y, (int)(rootPos.z + cornerPos.z) + z);
                    if (Evaluate3DNoise(b.position)) b.type = BlockType.Stone;
                    else
                    {
                        b.type = BlockType.Air;
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

        switch (position.y)
        {
            case 0: return true;
                //case 127: return false;
        }
        for (int octave = 0; octave < octaves; octave++)
        {
            noise += baseNoise.Evaluate(position * Mathf.Pow(lacunarity, octave) * frequency / 3) * Mathf.Pow(persistence, octave);
            divisor += Mathf.Pow(persistence, octave);
        }
        noise /= divisor;

        // float result = baseNoise.Evaluate(position * 0.0625f / 3);
        //if (position.y < 52) return true;
        if (-3.5 + position.y / 25f < noise)
            return true;
        // if (result > 0)
        return false;
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
                    if (block.type == BlockType.Stone)
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
        if (blockToCheck.type == BlockType.Air)
        {
            block.type = replacement;
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
