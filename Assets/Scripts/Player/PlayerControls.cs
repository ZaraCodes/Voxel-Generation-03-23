using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    #region References
    /// <summary>Reference to the character controller component</summary>
    public CharacterController CharCtrl;
    [SerializeField, Tooltip("Entity Collider")]
    private BoxCollider entityCollider;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private Generator generator;
    [SerializeField] private BlockType selectedBlock;
    /// <summary>
    /// Reference to the input system class
    /// </summary>
    private Controls controls;

    public Camera PlayerCamera;

    [SerializeField, Tooltip("The player's game mode")]
    private Gamemode gamemode;

    private bool flying;

    [SerializeField]
    int moveSpeed;

    [SerializeField]
    private float rotationSpeed;
    private float pitch;

    private float veloY = 0f;

    private Vector2 pitchMinMax = new Vector2(-89, 89);

    /// <summary>Defines if the player can move</summary>
    bool movementActive = true;

    [SerializeField] private TMP_Text debugText;

    [SerializeField] private SelectedCubeLineDrawer cubeDrawer;

    private bool placeBlock = false;
    private bool PlaceBlock { get { return placeBlock; } set { placeBlock = value; destroyBlock = false; } }

    private bool destroyBlock = false;
    private bool DestroyBlock { get { return destroyBlock; } set {  destroyBlock = value; placeBlock = false; } }

    private float breakCooldown = 0.1f;
    private float placeCooldown = 0.15f;
    private float blockCooldownTimer = 0f;
    #endregion

    #region Methods
    private void Awake()
    {
        // removes the cursor while playing and locks its position
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        // Debug.Log($"Round {40f / 16f + 0.01f} to {Mathf.RoundToInt(40f / 16f + 0.01f)}");
        // Debug.Log($"Round {24f / 16f + 0.01f} to {Mathf.RoundToInt(24f / 16f + 0.01f)}");
        controls = new();

        controls.Player.Hotkey1.performed += OnHotkey1;
        controls.Player.Hotkey2.performed += OnHotkey2;
        controls.Player.Hotkey3.performed += OnHotkey3;
        controls.Player.Hotkey4.performed += OnHotkey4;
        controls.Player.Hotkey5.performed += OnHotkey5;
        controls.Player.Hotkey6.performed += OnHotkey6;
        controls.Player.Hotkey7.performed += OnHotkey7;
        controls.Player.Hotkey8.performed += OnHotkey8;
        controls.Player.Hotkey9.performed += OnHotkey9;

    }

    // Update is called once per frame
    void Update()
    {
        if (movementActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                DestroyBlock = true;
            }
            if (Input.GetMouseButtonDown(1))
            {
                PlaceBlock = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                blockCooldownTimer = 0;
                DestroyBlock = false;
            }
            if (Input.GetMouseButtonUp(1))
            {
                blockCooldownTimer = 0;
                PlaceBlock = false;
            }
            Move();
            CameraMove();
            RaycastThing();
        }
    }

    private void RaycastThing()
    {
        debugText.text = $"FPS: {(int)(1 / Time.deltaTime)}\n";
        debugText.text += $"X: {Math.Round(transform.position.x, 4).ToString().PadRight(6, '0')}\tY: {Math.Round(transform.position.y, 4).ToString().PadRight(6, '0')}\tZ: {Math.Round(transform.position.z, 4).ToString().PadRight(6, '0')}\n";
        debugText.text += $"Hilliness: {GameManager.Instance.ChunkBuilder.EvaluateHilliness(transform.position)}\n";
        debugText.text += $"World Height: {GameManager.Instance.ChunkBuilder.EvaluateWorldHeight(transform.position)}\n";

        debugText.text += $"Grounded: {CharCtrl.isGrounded}\n";
        if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out RaycastHit hit, 6f))
        {
            Vector3Int blockPos;
            (float, float, float) normal = (hit.normal.x, hit.normal.y, hit.normal.z);
            Vector3 point = new(hit.point.x + 0.0001f, hit.point.y - 0.0001f, hit.point.z + 0.0001f);

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

            if (debugText.gameObject.activeInHierarchy)
            {
                debugText.text += $"Looking at\nX:{blockPos.x} Y:{blockPos.y} Z:{blockPos.z}\n";
                SubChunk targetSubChunk = hit.collider.GetComponent<SubChunk>();
                debugText.text += $"{targetSubChunk.GetBlock(blockPos).Type}\n";
            }
            cubeDrawer.CubePosition = blockPos;
            Debug.DrawLine(blockPos, blockPos + Vector3.forward);
            Debug.DrawLine(blockPos, blockPos + Vector3.right);
            Debug.DrawLine(blockPos, blockPos + Vector3.down);

            if (DestroyBlock)
            {
                if (blockCooldownTimer <= 0)
                {
                    blockCooldownTimer = breakCooldown;
                    SubChunk targetSubChunk = hit.collider.GetComponent<SubChunk>();
                    targetSubChunk.RemoveBlockAt(blockPos);
                }
                blockCooldownTimer -= Time.deltaTime;
            }
            else if (PlaceBlock)
            {
                if (blockCooldownTimer <= 0)
                {
                    blockCooldownTimer = placeCooldown;
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
                    Bounds bounds = new Bounds(new(blockPos.x + 0.5f, blockPos.y - 0.5f, blockPos.z + 0.5f), Vector3.one);
                    if (CharCtrl.bounds.Intersects(bounds))
                    {
                        Debug.Log("Cannot place block here");
                        return;
                    }
                    Vector2Int chunkPos = ChunkManager.Instance.GetChunkCoordinate(blockPos);
                    SubChunk.AddBlockAt(blockPos, selectedBlock, ChunkManager.Instance.GetChunk(chunkPos).GetComponent<Chunk>());
                }
                blockCooldownTimer -= Time.deltaTime;
            }
        }
        else if (cubeDrawer.CubeEnabled)
        {
            cubeDrawer.CubeEnabled = false;
        }
    }

    Bounds placedBlockBounds;
    private void showplacedaura()
    {
        if (placedBlockBounds != null)
        {
            Debug.DrawRay(placedBlockBounds.min, Vector3.up * placedBlockBounds.size.y);
            Debug.DrawRay(placedBlockBounds.min, Vector3.right * placedBlockBounds.size.x);
            Debug.DrawRay(placedBlockBounds.min, Vector3.forward * placedBlockBounds.size.z);

            Debug.DrawRay(placedBlockBounds.min + Vector3.right * placedBlockBounds.size.x, Vector3.forward * placedBlockBounds.size.z);
            Debug.DrawRay(placedBlockBounds.min + Vector3.right * placedBlockBounds.size.x, Vector3.up * placedBlockBounds.size.y);
            
            Debug.DrawRay(placedBlockBounds.max, Vector3.down * placedBlockBounds.size.y);
            Debug.DrawRay(placedBlockBounds.max, Vector3.left * placedBlockBounds.size.x);
            Debug.DrawRay(placedBlockBounds.max, Vector3.back * placedBlockBounds.size.z);
        }
    }

    /// <summary>
    /// Player Movement
    /// </summary>
    void Move()
    {
        Vector3 direction = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        direction = Vector3.ClampMagnitude(direction, 1) * moveSpeed;

        if (gamemode == Gamemode.Survival)
        {
            Walk(ref direction);
        }
        else if (gamemode == Gamemode.Creative)
        {
            if (flying) Fly(ref direction);
            else Walk(ref direction);
            if (Input.GetKeyDown(KeyCode.F))
            {
                flying = !flying;
                if (flying)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + .1f, transform.position.x);
                }
                else
                {
                    veloY = 0f;
                }
            }
        }

        CharCtrl.Move(direction * Time.deltaTime);
    }

    private void Walk(ref Vector3 direction)
    {
        if (CharCtrl.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            veloY = 5.03f;
            direction.y = veloY;
        }
        if (!CharCtrl.isGrounded)
        {
            veloY += -9.81f * Time.deltaTime;
            direction.y = veloY;
        }
        else if (veloY < 0f)
        {
            veloY = -1f;
            direction.y = veloY;
        }
    }

    private void Fly(ref Vector3 direction)
    {
        direction *= 1.5f;
        if (Input.GetKey(KeyCode.Space))
        {
            veloY = 6f;
            direction.y = veloY;
        }
        if (CharCtrl.isGrounded)
        {
            flying = false;
            veloY = 0f;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            veloY = -3f;
            direction.y = veloY;
        }
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

    private void OnHotkey1(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.Stone;
        }
    }

    private void OnHotkey2(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.Dirt;
        }
    }

    private void OnHotkey3(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.Grass;
        }
    }

    private void OnHotkey4(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.WoodPlanks;
        }
    }

    private void OnHotkey5(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.WoodLog;
        }
    }

    private void OnHotkey6(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {
            selectedBlock = BlockType.Leafes;
        }
    }

    private void OnHotkey7(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {

        }
    }

    private void OnHotkey8(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {

        }
    }
    private void OnHotkey9(InputAction.CallbackContext ctx)
    {
        if (ctx.action.WasPerformedThisFrame())
        {

        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    #endregion
}
