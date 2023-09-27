using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class Generator : MonoBehaviour
{
    /// <summary>Reference to the material that gets applied to the mesh</summary>
    [SerializeField] private Material mDefaultMaterial;

    [SerializeField] private AnimationCurve worldHeightCurve;

    /// <summary>Generated mesh</summary>
    private Mesh mesh;

    public ThreadedChunkBuilder threadedChunkBuilder;

    [SerializeField] private bool logPerformance;

    private void Awake()
    {
        threadedChunkBuilder = new(worldHeightCurve);
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

                    uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));
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

                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

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

                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

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

                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

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

                        uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

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

                    uvs.AddRange(UVSetter.GetUVs(blockAndItsFaces.blockType, direction));

                    break;
            }
        }
    }

    private IEnumerator PregenerateChunk(Vector3 rootPos, Transform parent, int size, int height)
    {
        GameObject generatedChunk = new();
        generatedChunk.transform.parent = parent;
        generatedChunk.transform.position = rootPos;

        Chunk chunk = generatedChunk.AddComponent<Chunk>();
        chunk.subChunks = new Block[height, size, size, size];
        chunk.ChunkPos = new((int)rootPos.x / size, (int)rootPos.z / size);
        generatedChunk.name = $"{chunk.ChunkPos.x}/{chunk.ChunkPos.y} pregenerated";

        Vector3 cornerPos = new(-size / 2f, 0f, -size / 2f);

        //stopwatch1 = new();
        //if (logPerformance)
        //    stopwatch1.Start();

        for (int level = 0; level < height; level++)
        {
            GameObject subChunk = new();
            subChunk.transform.parent = generatedChunk.transform;
            //subChunk.transform.position = new(0, 0, 0); //level * size
            //yield return null;
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
            blockDataTasks[level] = threadedChunkBuilder.StartGenerateBlockData(chunk, rootPos, cornerPos, level, size, ChunkManager.Instance.chunkOffsetY);
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
        ChunkManager.Instance.AddChunk(chunk.ChunkPos, generatedChunk);
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
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        if (chunkGeneration)
        {
            SubChunk sub;
            
            meshRenderer = subChunk.AddComponent<MeshRenderer>();
            meshFilter = subChunk.AddComponent<MeshFilter>();
            meshCollider = subChunk.AddComponent<MeshCollider>();
            sub = subChunk.AddComponent<SubChunk>();
            sub.Chunk = chunk;
        }
        else
        {
            meshRenderer = subChunk.GetComponent<MeshRenderer>();
            meshFilter = subChunk.GetComponent<MeshFilter>();
            meshCollider = subChunk.GetComponent<MeshCollider>();
        }

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

public enum BlockFace
{
    Yup, Ydown, Xup, Xdown, Zup, Zdown
}

public struct BlockAndItsFaces
{
    public Vector3 position;
    public BlockFace[] blockFaces;
    public BlockType blockType;
}

