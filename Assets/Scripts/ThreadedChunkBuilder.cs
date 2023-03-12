using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    private void BuildBlockSides(Chunk chunk, List<BlockAndItsFaces>[] chunkData, int level, int size, int height)
    {
        List<BlockAndItsFaces> subChunkData = new List<BlockAndItsFaces>();
        foreach (Block block in chunk.subChunks[level].Values)
        {
            if (block.type == "air") continue;
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
                if (!Evaluate3DNoise(new(block.position.x + blockSide.Item1, block.position.y + blockSide.Item2, block.position.z + blockSide.Item3)))
                {
                    //if (neighborBlock.type == "air")
                    {
                        AddBlockFace(faces, blockSide);
                    }
                }
                else
                {
                    if (!Evaluate3DNoise(new(block.position.x + blockSide.Item1, block.position.y + blockSide.Item2, block.position.z + blockSide.Item3)))
                    {
                        AddBlockFace(faces, blockSide);
                    }
                }
            }

            BlockFace[] blockFaces = faces.ToArray();
            BlockAndItsFaces blockAndItsFaces = new()
            {
                position = block.position,
                blockFaces = faces.ToArray()
            };
            subChunkData.Add(blockAndItsFaces);
        }
        chunkData[level] = subChunkData;
    }

    public Task StartGenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size)
    {
        return Task.Run(() => { GenerateBlockData(chunk, rootPos, cornerPos, level, size); });
    }

    private void GenerateBlockData(Chunk chunk, Vector3 rootPos, Vector3 cornerPos, int level, int size)
    {
        for (int x = (int)(rootPos.x + cornerPos.x); x < (int)(rootPos.x + cornerPos.x + size); x++)
        {
            for (int y = level * size; y < level * size + size; y++)
            {
                for (int z = (int)(rootPos.z + cornerPos.z); z < (int)(rootPos.z + cornerPos.z + size); z++)
                {
                    Block b = new();
                    b.position = new Vector3(x, y, z);
                    if (Evaluate3DNoise(b.position)) b.type = "solid";
                    else b.type = "air";
                    chunk.subChunks[level].Add($"{x}/{y}/{z}", b);
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
}
