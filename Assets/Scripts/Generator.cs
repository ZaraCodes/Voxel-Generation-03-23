using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public class Generator : MonoBehaviour
{
    /// <summary>Reference to the material that gets applied to the mesh</summary>
    [SerializeField, Tooltip("Reference to the default terrain material")]
    private Material mDefaultMaterial;

    [SerializeField, Tooltip("Reference to the water material")]
    private Material mWaterMaterial;

    [SerializeField] private AnimationCurve worldHeightCurve;
    public AnimationCurve WorldHeightCurve { get { return worldHeightCurve; } }

    /// <summary>Generated mesh</summary>
    private Mesh mesh;

    private Mesh waterMesh;

    public ThreadedChunkBuilder threadedChunkBuilder;

    [SerializeField] private bool logPerformance;

    private void Awake()
    {
        threadedChunkBuilder = new(worldHeightCurve);
    }

    public void GenerateFace(BlockAndItsFaces blockAndItsFaces, List<int> triangles, List<Vector3> vertices, List<Vector2> uvs, ref int vertIdx)
    {
        foreach (EBlockFace direction in blockAndItsFaces.blockFaces)
        {
            switch (direction)
            {
                case EBlockFace.Yup:
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

                    // fix x and z uwu
                    if (blockAndItsFaces.blockType == EBlockType.Water)
                    {
                        Vector2 p = new Vector2(blockAndItsFaces.position.x, blockAndItsFaces.position.z);

                        uvs.Add(p + Vector2.right);
                        uvs.Add(p + Vector2.up + Vector2.right);
                        uvs.Add(p + Vector2.up);
                        uvs.Add(p);
                    }
                    else
                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));
                    break;
                case EBlockFace.Ydown:
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

                        if (blockAndItsFaces.blockType == EBlockType.Water)
                        {
                            Vector2 p = new Vector2(rootPos.x, rootPos.z);

                            uvs.Add(p);
                            uvs.Add(p + Vector2.up);
                            uvs.Add(p + Vector2.up + Vector2.right);
                            uvs.Add(p + Vector2.right);
                        }
                        else
                            uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));
                        break;
                    }
                case EBlockFace.Xup:
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

                        if (blockAndItsFaces.blockType == EBlockType.Water)
                        {
                            uvs.Add(rootPos);
                            uvs.Add(rootPos + Vector3.down);
                            uvs.Add(rootPos + Vector3.forward + Vector3.down);
                            uvs.Add(rootPos + Vector3.forward);
                        }
                        else
                            uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

                        break;
                    }
                case EBlockFace.Xdown:
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

                        if (blockAndItsFaces.blockType == EBlockType.Water)
                        {
                            uvs.Add(rootPos + Vector3.forward + Vector3.up);
                            uvs.Add(rootPos + Vector3.forward);
                            uvs.Add(rootPos);
                            uvs.Add(rootPos + Vector3.up);
                        }
                        else
                            uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

                        break;
                    }
                case EBlockFace.Zup:
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

                        if (blockAndItsFaces.blockType == EBlockType.Water)
                        {
                            uvs.Add(rootPos + Vector3.up + Vector3.right);
                            uvs.Add(rootPos + Vector3.right);
                            uvs.Add(rootPos);
                            uvs.Add(rootPos + Vector3.up);
                        }
                        else
                            uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

                        break;
                    }
                case EBlockFace.Zdown:
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

                    if (blockAndItsFaces.blockType == EBlockType.Water)
                    {
                        uvs.Add(blockAndItsFaces.position);
                        uvs.Add(blockAndItsFaces.position + Vector3.down);
                        uvs.Add(blockAndItsFaces.position + Vector3.down + Vector3.right);
                        uvs.Add(blockAndItsFaces.position + Vector3.right);
                    }
                    else
                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

                    break;
            }
        }
    }

    private IEnumerator PregenerateChunk(Vector3 rootPos, Transform parent, int size, int height)
    {
        GenerateChunkObject(rootPos, parent, size, height, out GameObject chunkGO, out Chunk chunk);

        Vector3 cornerPos = new(-size / 2f, 0f, -size / 2f);

        //stopwatch1 = new();
        //if (logPerformance)
        //    stopwatch1.Start();

        for (int level = 0; level < height; level++)
        {
            GameObject subChunk = new();
            subChunk.name = $"Level {level}";
            subChunk.transform.parent = chunkGO.transform;
        }

        //if (logPerformance)
        //{
        //    stopwatch1.Stop();
        //    print($"Chunk {chunk.name} Chunk Object Creation: {stopwatch1.Elapsed.Milliseconds}");
        //    stopwatch1.Restart();
        //}
        Task[] blockDataTasks = new Task[height];
        for (int level = 0; level < height; level++)
        {
            blockDataTasks[level] = threadedChunkBuilder.StartGenerateBlockData(chunk, level, size, new(worldHeightCurve.keys));
        }
        yield return WaitForTasks(blockDataTasks, chunk);
        // Task.WaitAll(blockDataTasks);
        //if (logPerformance)
        //{
        //    stopwatch1.Stop();
        //    print($"Chunk {chunk.name} Block Generation: {stopwatch1.Elapsed.Milliseconds}");
        //}
        Task[] populationTasks = new Task[height];
        for (int level = 0; level < height; level++)
        {
            populationTasks[level] = threadedChunkBuilder.StartPopulateChunk(chunk, level, size, height);
        }
        yield return WaitForTasks(populationTasks, chunk);

        chunk.generationFinished = false;
        ChunkManager.Instance.AddChunk(chunk.ChunkPos, chunkGO);
    }

    private static void GenerateChunkObject(Vector3 rootPos, Transform parent, int size, int height, out GameObject chunkGO, out Chunk chunk)
    {
        chunkGO = new();
        chunkGO.transform.parent = parent;
        chunkGO.transform.position = rootPos;

        chunk = chunkGO.AddComponent<Chunk>();
        chunk.Transform = chunkGO.transform;
        chunk.Position = chunk.Transform.position;
        chunk.subChunks = new EBlockType[height, size, size, size];
        chunk.ChunkPos = new((int)rootPos.x / size, (int)rootPos.z / size);
        chunkGO.name = $"{chunk.ChunkPos.x}/{chunk.ChunkPos.y} pregenerated";
    }

    public IEnumerator GenerateChunkFromFile(Vector3 rootPos, Transform parent, int size, int height)
    {
        GenerateChunkObject(rootPos, parent, size, height, out GameObject chunkGO, out Chunk chunk);
        yield return null;
        // todo
    }

    public IEnumerator GenerateChunk(Vector3 rootPos, Transform parent, int size, int height, System.Action<GameObject> callback)
    {
        var generatedChunk = ChunkManager.Instance.GetChunk(ChunkManager.Instance.GetChunkCoordinate(rootPos));
        Chunk chunk = null;

        Stopwatch stopwatch1 = new();
        if (logPerformance)
            stopwatch1.Start();

        if (generatedChunk == null)
        {
            yield return PregenerateChunk(rootPos, parent, size, height);
            generatedChunk = ChunkManager.Instance.GetChunk(ChunkManager.Instance.GetChunkCoordinate(rootPos));
        }
        if (generatedChunk == null)
        {
            UnityEngine.Debug.Log("Chunk Error");
        }
        else
        {
            chunk = generatedChunk.GetComponent<Chunk>();
            Vector2Int[] neighborChunks = new Vector2Int[]
            {
                Vector2Int.down, Vector2Int.up, Vector2Int.left, Vector2Int.right
            };
            foreach (var neighbor in neighborChunks)
            {
                GameObject neighborChunk = ChunkManager.Instance.GetChunk(chunk.ChunkPos + neighbor);
                if (neighborChunk == null)
                {
                    yield return PregenerateChunk(new Vector3(rootPos.x + neighbor.x * size, 0, rootPos.z + neighbor.y * size), parent, size, height);
                }
            }
        }

        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Pregeneration: {stopwatch1.Elapsed.Milliseconds}");
            stopwatch1.Restart();
        }

        // Task.WaitAll(populationTasks);


        if (logPerformance)
            stopwatch1.Restart();
        Chunk[] neighborChunkObjects = new Chunk[]
{
            ChunkManager.Instance.GetChunk(new Vector2Int(chunk.ChunkPos.x + 1, chunk.ChunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunk.ChunkPos.x - 1, chunk.ChunkPos.y)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunk.ChunkPos.x, chunk.ChunkPos.y + 1)).GetComponent<Chunk>(),
            ChunkManager.Instance.GetChunk(new Vector2Int(chunk.ChunkPos.x, chunk.ChunkPos.y - 1)).GetComponent<Chunk>()
        };
        List<BlockAndItsFaces>[] chunkData = new List<BlockAndItsFaces>[height];
        Task[] blockSidesTasks = new Task[height];

        for (int level = 0; level < height; level++)
        {
            blockSidesTasks[level] = threadedChunkBuilder.StartBuildBlockSides(chunk, neighborChunkObjects, chunkData, level, size, height);
        }

        yield return WaitForTasks(blockSidesTasks, chunk);
        // Task.WaitAll(blockSidesTasks);
        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Face Finding: {stopwatch1.Elapsed.Milliseconds}");
        }

        if (logPerformance)
            stopwatch1.Restart();

        int num = 0;
        foreach (List<BlockAndItsFaces> subChunkBlockData in chunkData)
        {
            GenerateSubChunk(chunk, num, subChunkBlockData);
            num++;
        }
        if (logPerformance)
        {
            stopwatch1.Stop();
            print($"Chunk {chunk.name} Mesh Creation: {stopwatch1.Elapsed.Milliseconds}");
        }
        chunk.generationFinished = true;
        generatedChunk.name = $"{chunk.ChunkPos.x}/{chunk.ChunkPos.y}";
        callback.Invoke(generatedChunk);
    }

    public void GenerateSubChunk(Chunk chunk, int level, List<BlockAndItsFaces> subChunkBlockData, bool chunkGeneration = true)
    {
        GameObject subChunk = chunk.transform.GetChild(level).gameObject;
        GameObject waterObject;

        MeshRenderer terrainMeshRenderer;
        MeshFilter terrainMeshFilter;
        MeshCollider terrainMeshCollider;

        MeshRenderer waterMeshRenderer;
        MeshFilter waterMeshFilter;
        MeshCollider waterMeshCollider;

        if (chunkGeneration)
        {
            SubChunk sub;

            waterObject = new GameObject("Water Meshes");
            waterObject.transform.parent = subChunk.transform;

            terrainMeshRenderer = subChunk.AddComponent<MeshRenderer>();
            terrainMeshFilter = subChunk.AddComponent<MeshFilter>();
            terrainMeshCollider = subChunk.AddComponent<MeshCollider>();

            waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();
            waterMeshFilter = waterObject.AddComponent<MeshFilter>();
            waterMeshCollider = waterObject.AddComponent<MeshCollider>();

            sub = subChunk.AddComponent<SubChunk>();
            sub.Chunk = chunk;

        }
        else
        {
            waterObject = subChunk.transform.GetChild(0).gameObject;

            terrainMeshRenderer = subChunk.GetComponent<MeshRenderer>();
            terrainMeshFilter = subChunk.GetComponent<MeshFilter>();
            terrainMeshCollider = subChunk.GetComponent<MeshCollider>();

            waterMeshRenderer = waterObject.GetComponent<MeshRenderer>();
            waterMeshFilter = waterObject.GetComponent<MeshFilter>();
            waterMeshCollider = waterObject.GetComponent<MeshCollider>();

        }

        List<int> triangles = new();
        List<int> waterTris = new();
        List<Vector3> vertices = new();
        List<Vector3> waterVerts = new();
        List<Vector2> uvs = new();
        List<Vector2> WaterUvs = new();

        int vertIdx = 0;
        int waterVertIdx = 0;
        // int loopIdx = 0;
        foreach (BlockAndItsFaces blockData in subChunkBlockData)
        {
            if (blockData.blockType == EBlockType.Water)
                GenerateFace(blockData, waterTris, waterVerts, WaterUvs, ref waterVertIdx);
            else
                GenerateFace(blockData, triangles, vertices, uvs, ref vertIdx);
            // if (loopIdx++ % 100 == 0) yield return null;
        }
        // Debug.Log(loopIdx);

        mesh = new();

        mesh.Clear();
        mesh.name = "Collision Geometry";
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        waterMesh = new();
        waterMesh.Clear();
        waterMesh.name = "Water Geometry";
        waterMesh.vertices = waterVerts.ToArray();
        waterMesh.uv = WaterUvs.ToArray();
        waterMesh.triangles = waterTris.ToArray();
        waterMesh.RecalculateNormals();

        terrainMeshRenderer.material = mDefaultMaterial;
        terrainMeshFilter.sharedMesh = mesh;
        terrainMeshCollider.sharedMesh = mesh;

        waterMeshRenderer.material = mWaterMaterial;
        waterMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        waterMeshFilter.sharedMesh = waterMesh;
        waterMeshCollider.sharedMesh = waterMesh;
    }

    private IEnumerator WaitForTasks(Task[] tasks, Chunk chunk)
    {
        while (true)
        {
            bool continueWait = false;
            foreach (Task task in tasks)
                if (!task.IsCompleted)
                {
                    continueWait = true;
                    yield return null;
                    break;
                }
            if (!continueWait) break;
        }
    }
}

public enum EBlockFace
{
    Yup, Ydown, Xup, Xdown, Zup, Zdown
}

public struct BlockAndItsFaces
{
    public Vector3 position;
    public EBlockFace[] blockFaces;
    public EBlockType blockType;
}

