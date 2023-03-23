using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    #region References
    public CharacterController CharCtrl;

    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private Generator generator;

    public Camera PlayerCamera;
    [SerializeField]
    int moveSpeed;

    [SerializeField]
    private float rotationSpeed;
    private float pitch;

    private float veloY = 0f;

    private Vector2 pitchMinMax = new Vector2(-89, 89);

    /// <summary>Defines if the player can move</summary>
    bool movementActive = true;

    #endregion

    #region Functions
    private void Awake()
    {
        // removes the cursor while playing and locks its position
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"Round {2.5f} to {Mathf.RoundToInt(2.5f)}");
        Debug.Log($"Round {1.5f} to {Mathf.RoundToInt(1.5f)}");

    }

    // Update is called once per frame
    void Update()
    {
        if (movementActive)
        {
            Move();
            CameraMove();
            RaycastThing();
        }
    }

    private void RaycastThing()
    {
        if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out RaycastHit hit, 6f))
        {
            Vector3Int blockPos = new(Mathf.FloorToInt(hit.point.x), Mathf.CeilToInt(hit.point.y), Mathf.FloorToInt(hit.point.z));

            Debug.DrawLine(blockPos, blockPos + Vector3.forward);
            Debug.DrawLine(blockPos, blockPos + Vector3.right);
            Debug.DrawLine(blockPos, blockPos + Vector3.down);


            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(hit.normal);
                //hit.transform.gameObject.name


                Debug.Log("Original Pos:" + blockPos);
                Vector2Int chunkPos = chunkManager.GetChunkCoordinate(blockPos);
                //Debug.Log(chunkPos);
                GameObject chunkGameObject = chunkManager.GetChunk(chunkPos);

                if (chunkGameObject != null)
                {
                    Chunk chunk = chunkGameObject.GetComponent<Chunk>();
                    if (chunk == null)
                    {
                        Debug.Log("Chunk is null verySadge");
                        return;
                    }
                    int level = blockPos.y / 16;

                    blockPos = new(blockPos.x % 16, blockPos.y % 16, blockPos.z % 16);
                    blockPos += new Vector3Int(8, 0, 8);
                    if (blockPos.x < 0) blockPos.x += 16;
                    if (blockPos.z < 0) blockPos.z += 16;
                    blockPos = new(blockPos.x % 16, blockPos.y % 16, blockPos.z % 16);

                    Debug.Log("Chunk Pos: " + blockPos);
                    chunk.UpdateBlock(BlockType.Air, level, blockPos.x, blockPos.y, blockPos.z);


                    Chunk[] neighborChunks = new Chunk[]
                    {
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
                    };
                    List<BlockAndItsFaces> blockAndItsFaces = generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level, 16, chunkManager.chunkHeight);
                    generator.GenerateSubChunk(chunk, level, blockAndItsFaces, false);
                    if (blockPos.x == 0)
                        UpdateNeighborChunk(new(chunkPos.x - 1, chunkPos.y), level, neighborChunks, 1);
                    else if (blockPos.x == 16 - 1)
                        UpdateNeighborChunk(new(chunkPos.x + 1, chunkPos.y), level, neighborChunks, 0);
                    if (blockPos.z == 0)
                        UpdateNeighborChunk(new(chunkPos.x, chunkPos.y - 1), level, neighborChunks, 3);
                    else if (blockPos.z == 16 - 1)
                        UpdateNeighborChunk(new(chunkPos.x, chunkPos.y + 1), level, neighborChunks, 2);
                }
            }
        }
    }

    private void UpdateNeighborChunk(Vector2Int neighborChunkPos, int level, Chunk[] neighborChunks, int index)
    {
        List<BlockAndItsFaces> blockAndItsFaces;
        Chunk neighborChunk = neighborChunks[index];
        Chunk[] newNeighborChunks = new Chunk[]
        {
            chunkManager.GetChunk(new Vector2Int(neighborChunkPos.x + 1, neighborChunkPos.y)).GetComponent<Chunk>(),
            chunkManager.GetChunk(new Vector2Int(neighborChunkPos.x - 1, neighborChunkPos.y)).GetComponent<Chunk>(),
            chunkManager.GetChunk(new Vector2Int(neighborChunkPos.x, neighborChunkPos.y + 1)).GetComponent<Chunk>(),
            chunkManager.GetChunk(new Vector2Int(neighborChunkPos.x, neighborChunkPos.y - 1)).GetComponent<Chunk>()
        };
        blockAndItsFaces = generator.threadedChunkBuilder.BuildBlockSides(neighborChunk, newNeighborChunks, level, 16, chunkManager.chunkHeight);
        generator.GenerateSubChunk(neighborChunk, level, blockAndItsFaces, false);
    }

    /// <summary>
    /// Player Movement
    /// </summary>
    void Move()
    {
        Vector3 direction = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        direction = Vector3.ClampMagnitude(direction, 1) * moveSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            veloY = 4.03f;
            direction.y = veloY;
        }
        if (!CharCtrl.isGrounded)
        {
            veloY += -9.81f * Time.deltaTime;
            direction.y = veloY;
        }
        else if (veloY < 0f)
        {
            veloY = 0f;
            direction.y = veloY;
        }

        CharCtrl.Move(direction * Time.deltaTime);
    }

    /// <summary>
    /// Camera Movement
    /// </summary>
    void CameraMove()
    {
        transform.Rotate(Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime * Vector3.up);

        pitch += -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        PlayerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    /// <summary>Enables or disables movement</summary>
    /// <param name="active"></param>
    public void SetMovementActive(bool active)
    {
        movementActive = active;
    }

    public void LoadHappyEnd()
    {
        // SceneManager.LoadScene("Happy End");
    }
    #endregion
}
