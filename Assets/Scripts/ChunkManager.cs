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

    public int width;

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

    /// <summary>Updates the visible chunks each frame</summary>
    private IEnumerator UpdateChunks()
    {
        updatingChunks = true;
        List<Vector2Int> newActiveChunks = new();

        Vector2 currViewerPos = ViewerPos / chunkSize;
        Vector2Int currentViewerChunkCoord = new(
            Mathf.RoundToInt(currViewerPos.x),
            Mathf.RoundToInt(currViewerPos.y));

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
                if (!allChunkDic.ContainsKey(currChunkCoord))
                {
                    Vector3 currChunkWorldPos = new(currChunkCoord.x * chunkSize, 0, currChunkCoord.y * chunkSize);
                    // GameObject newChunkObj = generator.GenerateChunk(currChunkWorldPos, transform, width, chunkHeight);
                    GameObject newChunkObj = null;
                    yield return StartCoroutine(generator.GenerateChunk(currChunkWorldPos, transform, width, chunkHeight, returnValue =>
                    {
                        newChunkObj = returnValue;
                    }));
                    allChunkDic.Add(currChunkCoord, new ChunkPrivate(newChunkObj));
                }
                else allChunkDic[currChunkCoord].SetVisibility(true);
            }
            newActiveChunks.Add(currChunkCoord);
        }
        foreach (Vector2Int chunkInx in activeChunks)
        {
            allChunkDic[chunkInx].SetVisibility(false);
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
        if (SettingsManager.Instance.ViewDistance > 7)
        {
            SettingsManager.Instance.ViewDistance = 8;
        }
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
            yield return StartCoroutine(generator.GenerateChunk(new(pos.x * chunkSize, 0, pos.y * chunkSize), transform, width, chunkHeight, returnValue => { newChunkObj = returnValue; }));
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
                    Block block = spawnChunk.GetBlock(level, width / 2, y, width / 2);
                    if (block != null && block.type != BlockType.Air)
                    {
                        breaking = true;

                        // changes the player position
                        viewerTransform.parent.position = new(block.position.x + 0.5f, block.position.y, block.position.z + 0.5f);
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

    public Vector2Int GetChunkCoordinate(Vector3 position)
    {
        return new(Mathf.RoundToInt(position.x / chunkSize + 0.01f), Mathf.RoundToInt(position.z / chunkSize + 0.01f));
    }

    public GameObject GetChunk(Vector2Int chunkPos)
    {
        if (allChunkDic.TryGetValue(chunkPos, out var chunk))
        {
            return chunk.chunkObj;
        }
        return null;
    }

    /// <summary>Private Chunk class that holds necessary chunk data</summary>
    private class ChunkPrivate
    {
        /// <summary>The Chunk GameObject</summary>
        public GameObject chunkObj;

        /// <summary>Position of the chunk</summary>
        public Vector3 position;
        private Vector3 Position => position;

        /// <summary>Constructor of this Class</summary>
        /// <param name="chunkObj">The Chunk GameObject</param>
        public ChunkPrivate(GameObject chunkObj)
        {
            this.chunkObj = chunkObj;
            position = chunkObj.transform.position;
        }

        /// <summary>Makes a chunk visible or invisible</summary>
        /// <param name="isVisible">bool that sets this chunk's visibility</param>
        public void SetVisibility(bool isVisible)
        {
            chunkObj.SetActive(isVisible);
        }
    }

    private void Awake()
    {
        allChunkDic = new Dictionary<Vector2Int, ChunkPrivate>();
        activeChunks = new List<Vector2Int>();
        SettingsManager.Instance.ViewDistance = renderDistance;
        StartCoroutine(CreateSpawnArea());
        // generator.GenerateChunk(new(0, 0, 0), transform);
    }

    private void Update()
    {
        if (!updatingChunks)
            StartCoroutine(UpdateChunks());
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
                Gizmos.DrawWireCube(c.position, new Vector3(chunkSize, 0, chunkSize));
            }
        }
    }
}
