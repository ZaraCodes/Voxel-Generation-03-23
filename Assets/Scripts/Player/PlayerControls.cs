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
            if (Input.GetMouseButtonDown(0))
            {
                //hit.transform.gameObject.name
                Vector3 blockPos = new(Mathf.Floor(hit.point.x), Mathf.Ceil(hit.point.y), Mathf.Floor(hit.point.z));
                Debug.Log(blockPos);

                Vector2Int chunkPos = chunkManager.GetChunkCoordinate(blockPos);
                Debug.Log(chunkPos);
                GameObject chunkGameObject = chunkManager.GetChunk(chunkPos);

                if (chunkGameObject != null)
                {
                    Chunk chunk = chunkGameObject.GetComponent<Chunk>();
                    if (chunk == null)
                    {
                        Debug.Log("Chunk is null verySadge");
                        return;
                    }
                    int level = (int)blockPos.y / 16;
                    chunk.UpdateBlock(BlockType.Air, level, (int) blockPos.x % 16, (int) blockPos.y % 16, (int) blockPos.z % 16);


                    Chunk[] neighborChunks = new Chunk[]
                    {
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x + 1, chunkPos.y)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x - 1, chunkPos.y)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + 1)).GetComponent<Chunk>(),
                        chunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - 1)).GetComponent<Chunk>()
                    };
                    List<BlockAndItsFaces> blockAndItsFaces = generator.threadedChunkBuilder.BuildBlockSides(chunk, neighborChunks, level, 16, chunkManager.chunkHeight);
                    generator.GenerateSubChunk(chunk, level, blockAndItsFaces, false);
                }
            }
        }
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
