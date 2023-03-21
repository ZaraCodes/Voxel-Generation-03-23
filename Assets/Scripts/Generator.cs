using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Generator : MonoBehaviour
{
    /// <summary>Bool that decides if chunks get generated in a threaded way or not</summary>
    [SerializeField] private bool threaded;

    /// <summary>How many iterations of the outer for loop for generating a mesh will get calculated inside a thread instead</summary>
    [SerializeField] private int threadIterations;

    /// <summary>Reference to the water prefab</summary>
    [SerializeField] private GameObject waterPrefab;

    /// <summary>Reference to the tree prefab</summary>
    [SerializeField] private GameObject treePrefab;

    /// <summary>Reference to the material that gets applied to the mesh</summary>
    [SerializeField] private Material mDefaultMaterial;

    /// <summary>The strength that the resulting noise gets multiplied by</summary>
    [SerializeField] private float mNoiseStrength;

    /// <summary>The frequency of the noise</summary>
    [SerializeField] private float frequency;

    /// <summary>How often a noise gets layered to get a more varied result</summary>
    [SerializeField] private int octaves;

    /// <summary>Factor that scales each octave by this value down</summary>
    [SerializeField] private float lacunarity;

    /// <summary>Factor that reduces each octaves influence on the resulting noise</summary>
    [SerializeField] private float persistence;

    /// <summary>
    /// The noise for forests returns a value between 0 and 1. This value therefore has to be between these two values and can 
    /// make a tree appear if the calculated value is bigger than this threshhold.
    /// </summary>
    [SerializeField] private float forestThreshhold;

    /// <summary>Frequency of the forest noise</summary>
    [SerializeField] private float forestFrequency;

    /// <summary>The hilliness noise decides how much of the base noise gets applied. The hilliness frequency decides how big hilly and flat areas are.</summary>
    [SerializeField] private float hillinessFrequency;

    /// <summary>Frequency of the base height noise</summary>
    [SerializeField] private float baseHeightFequency;

    /// <summary>Sets how high the base height noise will be</summary>
    [SerializeField] private float baseHeightMultiplier;

    /// <summary>Base height that all other noise gets added to</summary>
    [SerializeField] private float baseHeight;

    /// <summary>Noise for the main terrain</summary>
    private SimplexNoise mainTerrainNoise;

    /// <summary>Noise for the base height</summary>
    private SimplexNoise baseHeightNoise;

    /// <summary>Noise for the hilliness</summary>
    private SimplexNoise hillinessNoise;

    /// <summary>Noise for forests</summary>
    private SimplexNoise woodinessNoise;

    /// <summary>Generated mesh</summary>
    private Mesh mesh;

    private ThreadedChunkBuilder threadedChunkBuilder;

    [SerializeField] private bool logPerformance;

    private void Awake()
    {
        SetSeed(Random.Range(int.MinValue, int.MaxValue));
        threadedChunkBuilder = new();
    }

    /// <summary>Sets the seeds for the different noises</summary>
    /// <param name="seed">the seed duh</param>
    private void SetSeed(int seed)
    {
        // GeneratorSettingsSingleton.Instance.seed = seed;
        mainTerrainNoise = new SimplexNoise(seed);
        baseHeightNoise = new SimplexNoise(seed++);
        hillinessNoise = new SimplexNoise(seed++);
        woodinessNoise = new SimplexNoise(seed++);
    }

    /// <summary>Generates a chunk</summary>
    /// <param name="rootPos">Root position of the chunk</param>
    /// <param name="resolution">Resolution of the chunk</param>
    /// <param name="chunkSize">Size of the chunk</param>
    /// <param name="parent">The object this chunk will get attached to</param>
    /// <returns>The generated chunk</returns>
    public GameObject GenerateChunk(Vector3 rootPos, int resolution, float chunkSize, Transform parent)
    {
        GameObject generatedTile = new();
        generatedTile.transform.position = rootPos;
        generatedTile.transform.SetParent(parent);
        generatedTile.name = $"Chunk_{rootPos.x}/{rootPos.z}";

        MeshRenderer tileRenderer = generatedTile.AddComponent<MeshRenderer>();
        MeshFilter tileFilter = generatedTile.AddComponent<MeshFilter>();
        MeshCollider tileCollider = generatedTile.AddComponent<MeshCollider>();

        mesh = new();
        mesh.name = $"ChunkMesh_{rootPos.x}/{rootPos.z}";

        tileRenderer.material = mDefaultMaterial;
        tileFilter.sharedMesh = mesh;

        GenerateMesh(rootPos, resolution, chunkSize);
        tileCollider.sharedMesh = mesh;

        return generatedTile;
    }

    /// <summary>Generates the chunk mesh</summary>
    /// <param name="rootPos">Root position of the chunk</param>
    /// <param name="resolution">Resolution of the chunk</param>
    /// <param name="size">Size of the chunk</param>
    private void GenerateMesh(Vector3 rootPos, int resolution, float size)
    {
        Vector3[] verts = new Vector3[(resolution + 1) * (resolution + 1)];
        int[] tris = new int[resolution * resolution * 2 * 3];

        Vector3 cornerPos = new(-size / 2f, 0f, -size / 2f);

        if (threaded)
        {
            List<Task> tasks = new();
            for (int y = 0, vertIdx = 0, triIdx = 0; y <= resolution; y += threadIterations, vertIdx += (resolution + 1) * threadIterations, triIdx += resolution * 6 * threadIterations)
            {
                tasks.Add(StartInnerForThread(rootPos, resolution, size, verts, tris, cornerPos, y, vertIdx, triIdx));
            }
            Task.WaitAll(tasks.ToArray());
        }
        else
        {
            for (int y = 0, vertIdx = 0, triIdx = 0; y <= resolution; y++, vertIdx += resolution + 1, triIdx += resolution * 6)
            {
                InnerForLoopNormal(rootPos, resolution, size, verts, tris, cornerPos, y, vertIdx, triIdx);
            }
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    /// <summary>Starts a thread for threaded chunk generation</summary>
    /// <param name="rootPos">Root position of the chunk</param>
    /// <param name="resolution">Resolution of the chunk</param>
    /// <param name="size">Size of the chunk</param>
    /// <param name="verts">Array of all verts for this chunk</param>
    /// <param name="tris">Array of all tris for this chunk</param>
    /// <param name="cornerPos">Corner position of the chunk</param>
    /// <param name="y">y parameter of the outer for loop</param>
    /// <param name="vertIdx">vertIdx from the outer for loop</param>
    /// <param name="triIdx">triIdx from the outer for loop</param>
    /// <returns>A task</returns>
    private Task StartInnerForThread(Vector3 rootPos, int resolution, float size, Vector3[] verts, int[] tris, Vector3 cornerPos, int y, int vertIdx, int triIdx)
    {
        return Task.Run(() => InnerForLoopThreaded(rootPos, resolution, size, verts, tris, cornerPos, y, vertIdx, triIdx));
    }

    /// <summary>generates a chunk in a non threaded way</summary>
    /// <param name="rootPos">Root position of the chunk</param>
    /// <param name="resolution">Resolution of the chunk</param>
    /// <param name="size">Size of the chunk</param>
    /// <param name="verts">Array of all verts for this chunk</param>
    /// <param name="tris">Array of all tris for this chunk</param>
    /// <param name="cornerPos">Corner position of the chunk</param>
    /// <param name="y">y parameter of the outer for loop</param>
    /// <param name="vertIdx">vertIdx from the outer for loop</param>
    /// <param name="triIdx">triIdx from the outer for loop</param>
    private void InnerForLoopNormal(Vector3 rootPos, int resolution, float size, Vector3[] verts, int[] tris, Vector3 cornerPos, int y, int vertIdx, int triIdx)
    {
        for (int x = 0; x <= resolution; x++, vertIdx++)
        {
            Vector3 vertPosLocal = cornerPos + (new Vector3(x, 0, y) / resolution) * size;
            Vector3 vertPosWorld = rootPos + vertPosLocal;

            vertPosLocal.y = EvaluateCoordinateHeight(vertPosWorld);

            if (vertIdx < verts.Length) verts[vertIdx] = vertPosLocal;

            if (x < resolution && y < resolution)
            {
                tris[triIdx + 0] = vertIdx;
                tris[triIdx + 1] = vertIdx + (resolution + 1) + 1;
                tris[triIdx + 2] = vertIdx + 1;

                tris[triIdx + 3] = vertIdx;
                tris[triIdx + 4] = vertIdx + (resolution + 1);
                tris[triIdx + 5] = vertIdx + (resolution + 1) + 1;

                triIdx += 6;
            }
        }
    }

    /// <summary>Generates a chunk in a threded way</summary>
    /// <param name="rootPos">Root position of the chunk</param>
    /// <param name="resolution">Resolution of the chunk</param>
    /// <param name="size">Size of the chunk</param>
    /// <param name="verts">Array of all verts for this chunk</param>
    /// <param name="tris">Array of all tris for this chunk</param>
    /// <param name="cornerPos">Corner position of the chunk</param>
    /// <param name="y">y parameter of the outer for loop</param>
    /// <param name="vertIdx">vertIdx from the outer for loop</param>
    /// <param name="triIdx">triIdx from the outer for loop</param>
    private void InnerForLoopThreaded(Vector3 rootPos, int resolution, float size, Vector3[] verts, int[] tris, Vector3 cornerPos, int y, int vertIdx, int triIdx)
    {
        for (int i = 0; i < threadIterations; i++)
        {
            for (int x = 0; x <= resolution; x++, vertIdx++)
            {
                Vector3 vertPosLocal = cornerPos + (new Vector3(x, 0, y) / resolution) * size;
                Vector3 vertPosWorld = rootPos + vertPosLocal;

                vertPosLocal.y = EvaluateCoordinateHeight(vertPosWorld);
                // if (vertPosLocal.y < minHeight) minHeight = vertPosLocal.y;

                verts[vertIdx] = vertPosLocal;
                // else Debug.Log($"vIdx: {vertIdx} x: {x} y: {y}");

                if (x < resolution && y < resolution)
                {
                    tris[triIdx + 0] = vertIdx;
                    tris[triIdx + 1] = vertIdx + (resolution + 1) + 1;
                    tris[triIdx + 2] = vertIdx + 1;

                    tris[triIdx + 3] = vertIdx;
                    tris[triIdx + 4] = vertIdx + (resolution + 1);
                    tris[triIdx + 5] = vertIdx + (resolution + 1) + 1;

                    triIdx += 6;
                }
            }

            y++;
            if (y > resolution) return;
        }
    }

    /// <summary>Calculates the height of the terrain at a certain point</summary>
    /// <param name="position">Position in the world</param>
    /// <returns>The height of the terrain</returns>
    public float EvaluateCoordinateHeight(Vector3 position)
    {
        position /= 13;

        float noise = 0;
        float divisor = 0;
        for (int i = 0; i < octaves; i++)
        {
            noise += mainTerrainNoise.Evaluate(new(position.x * Mathf.Pow(lacunarity, i) * frequency, 0f, position.z * Mathf.Pow(lacunarity, i) * frequency)) * Mathf.Pow(persistence, i);
            divisor += Mathf.Pow(persistence, i);
        }

        noise /= divisor;
        float worldHeight = baseHeight + baseHeightNoise.Evaluate(new(position.x * frequency * baseHeightFequency, 0f, position.z * frequency * baseHeightFequency)) * baseHeightMultiplier;
        worldHeight += noise * mNoiseStrength * ((hillinessNoise.Evaluate(new(position.z * frequency * hillinessFrequency, 0f, position.x * frequency * hillinessFrequency)) + 1) / 2);

        return worldHeight;
    }

    public void GenerateFace(BlockAndItsFaces blockAndItsFaces, List<int> triangles, List<Vector3> vertices, List<Vector2> uvs, ref int vertIdx)
    {
        foreach (BlockFace direction in blockAndItsFaces.blockFaces)
        {
            switch (direction)
            {
                case BlockFace.Yup:
                    vertices.Add(blockAndItsFaces.position + Vector3.right);
                    vertices.Add(blockAndItsFaces.position + Vector3.forward + Vector3.right);
                    vertices.Add(blockAndItsFaces.position + Vector3.forward);
                    vertices.Add(blockAndItsFaces.position);

                    triangles.Add(vertIdx);
                    triangles.Add(vertIdx + 2);
                    triangles.Add(vertIdx + 1);
                    triangles.Add(vertIdx);
                    triangles.Add(vertIdx + 3);
                    triangles.Add(vertIdx + 2);

                    vertIdx += 4;

                    uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));
                    break;
                case BlockFace.Ydown:
                    {
                        Vector3 rootPos = blockAndItsFaces.position + Vector3.down;
                        vertices.Add(rootPos);
                        vertices.Add(rootPos + Vector3.forward);
                        vertices.Add(rootPos + Vector3.forward + Vector3.right);
                        vertices.Add(rootPos + Vector3.right);

                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 2);
                        triangles.Add(vertIdx + 1);
                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 3);
                        triangles.Add(vertIdx + 2);

                        vertIdx += 4;

                        uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));

                        break;
                    }
                case BlockFace.Xup:
                    {
                        Vector3 rootPos = blockAndItsFaces.position + Vector3.right;

                        vertices.Add(rootPos);
                        vertices.Add(rootPos + Vector3.down);
                        vertices.Add(rootPos + Vector3.forward + Vector3.down);
                        vertices.Add(rootPos + Vector3.forward);

                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 2);
                        triangles.Add(vertIdx + 1);
                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 3);
                        triangles.Add(vertIdx + 2);
                        vertIdx += 4;

                        uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));

                        break;
                    }
                case BlockFace.Xdown:
                    {
                        Vector3 rootPos = blockAndItsFaces.position + Vector3.down;
                        vertices.Add(rootPos + Vector3.forward + Vector3.up);
                        vertices.Add(rootPos + Vector3.forward);
                        vertices.Add(rootPos);
                        vertices.Add(rootPos + Vector3.up);

                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 2);
                        triangles.Add(vertIdx + 1);
                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 3);
                        triangles.Add(vertIdx + 2);
                        vertIdx += 4;

                        uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));

                        break;
                    }
                case BlockFace.Zup:
                    {
                        Vector3 rootPos = blockAndItsFaces.position + Vector3.forward + Vector3.down;

                        vertices.Add(rootPos + Vector3.up + Vector3.right);
                        vertices.Add(rootPos + Vector3.right);
                        vertices.Add(rootPos);
                        vertices.Add(rootPos + Vector3.up);

                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 2);
                        triangles.Add(vertIdx + 1);
                        triangles.Add(vertIdx);
                        triangles.Add(vertIdx + 3);
                        triangles.Add(vertIdx + 2);
                        vertIdx += 4;

                        uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));

                        break;
                    }
                case BlockFace.Zdown:
                    vertices.Add(blockAndItsFaces.position);
                    vertices.Add(blockAndItsFaces.position + Vector3.down);
                    vertices.Add(blockAndItsFaces.position + Vector3.down + Vector3.right);
                    vertices.Add(blockAndItsFaces.position + Vector3.right);

                    triangles.Add(vertIdx);
                    triangles.Add(vertIdx + 2);
                    triangles.Add(vertIdx + 1);
                    triangles.Add(vertIdx);
                    triangles.Add(vertIdx + 3);
                    triangles.Add(vertIdx + 2);
                    vertIdx += 4;

                    uvs.AddRange(UVSetter.GetUVs(BlockType.Stone));

                    break;
            }
        }
    }

    public IEnumerator GenerateChunk(Vector3 rootPos, Transform parent, int size, int height, System.Action<GameObject> callback)
    {
        GameObject generatedChunk = new();
        generatedChunk.transform.parent = parent;
        generatedChunk.transform.position = rootPos;
        generatedChunk.name = $"{rootPos.x / size}/{rootPos.z / size}";
        Chunk chunk = generatedChunk.AddComponent<Chunk>();
        chunk.subChunks = new Block[height, size, size, size];

        Vector3 cornerPos = new(-size / 2f, 0f, -size / 2f);

        Stopwatch stopwatch1 = new();
        if (logPerformance)
            stopwatch1.Start();

        for (int level = 0; level < height; level++)
        {
            GameObject subChunk = new();
            subChunk.transform.parent = generatedChunk.transform;
            //subChunk.transform.position = new(0, 0, 0); //level * size
            //yield return null;
        }

        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Chunk Object Creation: {stopwatch1.Elapsed.Milliseconds}");
            stopwatch1.Restart();
        }

        Task[] blockDataTasks = new Task[height];
        for (int level = 0; level < height; level++)
        {
            blockDataTasks[level] = threadedChunkBuilder.StartGenerateBlockData(chunk, rootPos, cornerPos, level, size);
        }
        Task.WaitAll(blockDataTasks);
        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Block Generation: {stopwatch1.Elapsed.Milliseconds}");
        }
        yield return null;
        
        if (logPerformance)
            stopwatch1.Restart();
        List<BlockAndItsFaces>[] chunkData = new List<BlockAndItsFaces>[height];
        Task[] blockSidesTasks = new Task[height];
        for (int level = 0; level < height; level++)
        {
            blockSidesTasks[level] = threadedChunkBuilder.StartBuildBlockSides(chunk, chunkData, level, size, height);
        }
        Task.WaitAll(blockSidesTasks);
        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Face Finding: {stopwatch1.Elapsed.Milliseconds}");
        }
        yield return null;

        if (logPerformance)
            stopwatch1.Restart();
        int num = 0;
        foreach (List<BlockAndItsFaces> subChunkBlockData in chunkData)
        {
            GameObject subChunk = chunk.transform.GetChild(num).gameObject;
            MeshRenderer meshRenderer = subChunk.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = subChunk.AddComponent<MeshFilter>();
            MeshCollider meshCollider = subChunk.AddComponent<MeshCollider>();

            List<int> triangles = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            int vertIdx = 0;
            // int loopIdx = 0;
            foreach (BlockAndItsFaces blockData in subChunkBlockData)
            {
                GenerateFace(blockData, triangles, vertices, uvs, ref vertIdx);
                // if (loopIdx++ % 100 == 0) yield return null;
            }
            // Debug.Log(loopIdx);

            mesh = new();

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            meshRenderer.material = mDefaultMaterial;
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;

            num++;
            // yield return null;
        }
        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Mesh Creation: {stopwatch1.Elapsed.Milliseconds}");
        }
        callback.Invoke(generatedChunk);
    }
}

public enum BlockFace
{
    Yup, Ydown, Xup, Xdown, Zup, Zdown
}

public struct BlockAndItsFaces
{
    public Vector3 position;
    public BlockFace[] blockFaces;
}

