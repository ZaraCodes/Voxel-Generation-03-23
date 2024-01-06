using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    private static ChunkManager instance;
    public static ChunkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.Find("ChunkManager").GetComponent<ChunkManager>();
            }
            return instance;
        }
    }

    /// <summary>Reference to the generator</summary>
    [SerializeField] private Generator generator;
    public Generator Generator { get { return generator; } }

    /// <summary>Reference to the viewer transform which is used to determine the active chunks</summary>
    [SerializeField] private Transform viewerTransform;

    /// <summary>Determines how many chunks are visible</summary>
    [SerializeField] private int renderDistance;

    /// <summary>Sets how big a chunk is</summary>
    [SerializeField] private float chunkSize;

    /// <summary>The resolution determines how detailed a chunk mesh will be. The higher the value, the more detailed a chunk is</summary>
    [SerializeField] private int resolution;

    public int chunkOffsetY;

    public int Width { get; private set; }

    public int chunkHeight;

    private bool updatingChunks = false;

    /// <summary>Dictionary of all the generated chunks</summary>
    private Dictionary<Vector2Int, ChunkPrivate> allChunkDic;

    /// <summary>This list keeps track of all active and visible chunks</summary>
    private List<Vector2Int> activeChunks;

    /// <summary>The position of the viewer transform</summary>
    private Vector2 ViewerPos => new(viewerTransform.position.x, viewerTransform.position.z);

    [SerializeField] private RectTransform loadingBar;

    [SerializeField] private TMP_Text loadingText;

    [SerializeField] private GameObject loadingScreen;

    private bool stopGeneratingChunks;

    private Coroutine chunkUpdateCoroutine;

    private Vector2Int prevViewerChunkCoord;

    /// <summary>Updates the visible chunks each frame</summary>
    private IEnumerator UpdateChunks(Vector2Int currentViewerChunkCoord)
    {
        prevViewerChunkCoord = currentViewerChunkCoord;
        updatingChunks = true;
        List<Vector2Int> newActiveChunks = new();

        foreach (Vector2Int pos in SettingsManager.Instance.ViewArea)
        {
            Vector2Int currChunkCoord = currentViewerChunkCoord + pos;

            if (activeChunks.Contains(currChunkCoord))
            {
                /*Wir wissen:
                 *- er ist an
                 *- er ist generiert
                 */
                activeChunks.Remove(currChunkCoord);
            }
            else
            {
                if (stopGeneratingChunks) continue;
                if (false && SaveManager.DoesChunkExist(currChunkCoord)) {
                    SaveManager.LoadChunk();
                }
                else if (!allChunkDic.ContainsKey(currChunkCoord) || !allChunkDic[currChunkCoord].ChunkObj.GetComponent<Chunk>().GenerationFinished)
                {
                    Vector3 currChunkWorldPos = new(currChunkCoord.x * chunkSize, 0, currChunkCoord.y * chunkSize);
                    GameObject newChunkObj = null;
                    yield return StartCoroutine(generator.GenerateChunk(currChunkWorldPos, transform, Width, chunkHeight, returnValue =>
                    {
                        newChunkObj = returnValue;
                    }));
                    if (!allChunkDic.ContainsKey(currChunkCoord)) allChunkDic.Add(currChunkCoord, new ChunkPrivate(newChunkObj));
                }
                else allChunkDic[currChunkCoord].SetVisibility(true);
            }
            newActiveChunks.Add(currChunkCoord);
        }
        foreach (Vector2Int chunkInx in activeChunks)
        {
            allChunkDic[chunkInx].SetVisibility(false);
            Chunk chunk = allChunkDic[chunkInx].ChunkObj.GetComponent<Chunk>();
            SaveManager.SaveChunk(chunk);
            //allChunkDic.Remove(chunkInx);
        }
        activeChunks = newActiveChunks;
        updatingChunks = false;
    }

    /// <summary>Generate a small area of a new world to spawn the player in</summary>
    public IEnumerator CreateSpawnArea()
    {
        updatingChunks = true;
        GameManager.Instance.IsLoading = true;
        loadingScreen.SetActive(true);
        float barLength = 900;
        Chunk spawnChunk = null;
        int viewDistanceCache = SettingsManager.Instance.ViewDistance;

        viewerTransform.parent.gameObject.GetComponent<CharacterController>().enabled = false;
        viewerTransform.parent.gameObject.GetComponent<PlayerControls>().SetMovementActive(false);

        SettingsManager.Instance.ViewDistance = 4;

        int numberOfSteps = SettingsManager.Instance.ViewArea.Length;
        float step = 0;
        loadingBar.sizeDelta = new(barLength * (step / numberOfSteps), 30);
        yield return null;
        foreach (Vector2Int pos in SettingsManager.Instance.ViewArea)
        {
            step++;
            loadingText.text = $"Generating Chunks... {(int)(step * 100 / numberOfSteps)}%";
            loadingBar.sizeDelta = new(barLength * (step / numberOfSteps), 30);
            GameObject newChunkObj = null;
            yield return StartCoroutine(generator.GenerateChunk(new(pos.x * chunkSize, 0, pos.y * chunkSize), transform, Width, chunkHeight, returnValue => { newChunkObj = returnValue; }));
            if (!allChunkDic.ContainsKey(pos))
                allChunkDic.Add(pos, new ChunkPrivate(newChunkObj));
            activeChunks.Add(pos);
            if (pos.x == 0 && pos.y == 0)
                spawnChunk = newChunkObj.GetComponent<Chunk>();
        }
        SettingsManager.Instance.ViewDistance = viewDistanceCache;

        if (spawnChunk != null)
        {
            for (int level = chunkHeight - 1; level >= 0; level--)
            {
                bool breaking = false;
                for (int y = 15; y >= 0; y--)
                {
                    var block = spawnChunk.GetBlock(level, Width / 2, y, Width / 2);
                    if (block != null && block != EBlockType.Air)
                    {
                        breaking = true;

                        // changes the player position
                        viewerTransform.parent.position = new(.5f, GetBlockY(level, y), .5f);
                        viewerTransform.parent.gameObject.GetComponent<CharacterController>().enabled = true;
                        viewerTransform.parent.gameObject.GetComponent<PlayerControls>().SetMovementActive(true);
                        break;
                    }
                }
                if (breaking) break;
            }
        }
        updatingChunks = false;
        loadingScreen.SetActive(false);
        GameManager.Instance.IsLoading = false;
    }

    public int GetBlockY(int level, int y) => (int) (level * chunkSize + y - chunkOffsetY * chunkSize);
    public Vector2Int GetChunkCoordinate(Vector3 position)
    {
        return new(Mathf.RoundToInt(position.x / chunkSize + 0.01f), Mathf.RoundToInt(position.z / chunkSize + 0.01f));
    }

    public GameObject GetChunk(Vector2Int chunkPos)
    {
        if (allChunkDic.TryGetValue(chunkPos, out var chunk))
        {
            return chunk.ChunkObj;
        }
        return null;
    }

    /// <summary>Adds a chunk to the dictionary</summary>
    /// <param name="pos"></param>
    /// <param name="chunk"></param>
    public void AddChunk(Vector2Int pos, GameObject chunk)
    {
        if (!allChunkDic.ContainsKey(pos))
        {
            allChunkDic.Add(pos, new ChunkPrivate(chunk));
        }
    }

    /// <summary>Private Chunk class that holds necessary chunk data</summary>
    private class ChunkPrivate
    {
        /// <summary>The Chunk GameObject</summary>
        public GameObject ChunkObj;

        /// <summary>Position of the chunk</summary>
        public Vector3 Position;

        /// <summary>Constructor of this Class</summary>
        /// <param name="chunkObj">The Chunk GameObject</param>
        public ChunkPrivate(GameObject chunkObj)
        {
            this.ChunkObj = chunkObj;
            Position = chunkObj.transform.position;
        }

        /// <summary>Makes a chunk visible or invisible</summary>
        /// <param name="isVisible">bool that sets this chunk's visibility</param>
        public void SetVisibility(bool isVisible)
        {
            ChunkObj.SetActive(isVisible);
        }
    }

    private void Awake()
    {
        allChunkDic = new Dictionary<Vector2Int, ChunkPrivate>();
        activeChunks = new List<Vector2Int>();
        Width = (int)chunkSize;
        SettingsManager.Instance.ViewDistance = renderDistance;
        StartCoroutine(CreateSpawnArea());
        // generator.GenerateChunk(new(0, 0, 0), transform);
    }

    private void Update()
    {
        Vector2 currViewerPos = ViewerPos / chunkSize;
        Vector2Int currentViewerChunkCoord = new(Mathf.RoundToInt(currViewerPos.x), Mathf.RoundToInt(currViewerPos.y));

        if (!updatingChunks)
        {
            stopGeneratingChunks = false;
            chunkUpdateCoroutine = StartCoroutine(UpdateChunks(currentViewerChunkCoord));
        }
        else
        {
            if (currentViewerChunkCoord != prevViewerChunkCoord)
            {
                stopGeneratingChunks = true;
            }
        }

        // generator.GenerateChunk(new(0, 0, 0), transform);
    }

    /// <summary>Draws outlines of all generated chunks in the editor</summary>
    private void OnDrawGizmos()
    {
        if (allChunkDic != null)
        {
            Gizmos.color = Color.green;
            foreach (KeyValuePair<Vector2Int, ChunkPrivate> entry in allChunkDic)
            {
                ChunkPrivate c = entry.Value;
                Gizmos.DrawWireCube(c.Position, new Vector3(chunkSize, 0, chunkSize));
            }
        }
    }
}
