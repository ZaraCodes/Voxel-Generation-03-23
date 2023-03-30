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

    [SerializeField] TMPro.TMP_Text debugText;
    #endregion

    #region Functions
    private void Awake()
    {
        // removes the cursor while playing and locks its position
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        // Debug.Log($"Round {40f / 16f + 0.01f} to {Mathf.RoundToInt(40f / 16f + 0.01f)}");
        // Debug.Log($"Round {24f / 16f + 0.01f} to {Mathf.RoundToInt(24f / 16f + 0.01f)}");

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
        debugText.text = $"FPS: {(int)(1 / Time.deltaTime)}\n";
        if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out RaycastHit hit, 6f))
        {
            Vector3Int blockPos;
            (float, float, float) normal = (hit.normal.x, hit.normal.y, hit.normal.z);
            Vector3 point = new(hit.point.x + 0.0001f, hit.point.y - 0.0001f, hit.point.z + 0.0001f);

            if (debugText.gameObject.activeInHierarchy)
                debugText.text += $"X:{point.x}\nY:{point.y}\nZ:{point.z}";

            switch (normal)
            {
                case (1, 0, 0):
                    blockPos = new(Mathf.FloorToInt(point.x - 1), Mathf.CeilToInt(point.y), Mathf.FloorToInt(point.z));
                    break;
                case (0, -1, 0):
                    blockPos = new(Mathf.FloorToInt(point.x), Mathf.CeilToInt(point.y + 1), Mathf.FloorToInt(point.z));
                    break;
                case (0, 0, 1):
                    blockPos = new(Mathf.FloorToInt(point.x), Mathf.CeilToInt(point.y), Mathf.FloorToInt(point.z - 1));
                    break;
                default:
                    blockPos = new(Mathf.FloorToInt(point.x), Mathf.CeilToInt(point.y), Mathf.FloorToInt(point.z));
                    break;
            }

            Debug.DrawLine(blockPos, blockPos + Vector3.forward);
            Debug.DrawLine(blockPos, blockPos + Vector3.right);
            Debug.DrawLine(blockPos, blockPos + Vector3.down);


            if (Input.GetMouseButtonDown(0))
            {
                SubChunk targetSubChunk = hit.collider.GetComponent<SubChunk>();
                targetSubChunk.RemoveBlockAt(blockPos);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                switch (normal)
                {
                    case (1, 0, 0):
                        blockPos.x += 1; break;
                    case (-1, 0, 0):
                        blockPos.x -= 1; break;
                    case (0, 1, 0):
                        blockPos.y += 1; break;
                    case (0, -1, 0):
                        blockPos.y -= 1; break;
                    case (0, 0, 1):
                        blockPos.z += 1; break;
                    case (0, 0, -1):
                        blockPos.z -= 1; break;
                }
                Vector2Int chunkPos = ChunkManager.Instance.GetChunkCoordinate(blockPos);
                SubChunk.AddBlockAt(blockPos, BlockType.WoodPlanks, ChunkManager.Instance.GetChunk(chunkPos).GetComponent<Chunk>());
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
        transform.Rotate(Input.GetAxis("Mouse X") * rotationSpeed * Vector3.up);

        pitch += -Input.GetAxis("Mouse Y") * rotationSpeed;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        PlayerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    /// <summary>Enables or disables movement</summary>
    /// <param name="active"></param>
    public void SetMovementActive(bool active)
    {
        movementActive = active;
    }
    #endregion
}
