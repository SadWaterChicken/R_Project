using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementStateMachine : StateManager<PlayerMovementStateMachine.PlayerState>
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Sprint,
        Jump,
        Dash
    }

    [Header("Component References")]
    public CharacterController characterController;
    public Animator PlayerAnimator;
    public PlayerStat playerStat;
    public InputSystem_Actions inputActions;
    public CinemachineCamera Camera;
    public EquipmentManager equipmentManager;
    public SpriteRenderer spriteRenderer;
    public PlayerCombatStateMachine playerCombatStateMachine;

    [Header("Movement Settings")]
    public float gravity = 15f;
    public Vector3 currentVelocity;
    public float currentSpeed;

    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách tối đa để phát hiện và tương tác với vật thể (IInteractable)")]
    public float interactionRange = 3f;
    [Tooltip("Giao diện gợi ý tương tác (ví dụ: 'Nhấn F để tương tác')")]
    public GameObject interactHintUI;
    private IInteractable nearbyInteractable;

    private static PlayerMovementStateMachine instance;
    public static PlayerMovementStateMachine Instance => instance;

    /// <summary>
    /// Mục đích: Khởi tạo Singleton, giữ nhân vật không bị hủy khi chuyển Scene (DontDestroyOnLoad),
    /// khởi tạo hệ thống Input System và thiết lập danh sách các State di chuyển.
    /// </summary>
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
        gameObject.tag = "Player";

        inputActions = new InputSystem_Actions();

        States.Add(PlayerState.Idle, new PlayerIdleState(PlayerState.Idle, this));
        States.Add(PlayerState.Walk, new PlayerWalkState(PlayerState.Walk, this));
        States.Add(PlayerState.Sprint, new PlayerSprintState(PlayerState.Sprint, this));
        States.Add(PlayerState.Jump, new PlayerJumpState(PlayerState.Jump, this));
        States.Add(PlayerState.Dash, new PlayerDashState(PlayerState.Dash, this));

        CurrentState = States[PlayerState.Idle];
    }

    /// <summary>
    /// Mục đích: Kiểm tra và phục hồi vị trí của người chơi khi quay trở về từ Dungeon thông qua PlayerPrefs.
    /// </summary>
    void Start()
    {
        CheckReturnPositionFromDungeon();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }

    /// <summary>
    /// Mục đích: Vòng lặp cập nhật chính của State Machine:
    /// 1. Cập nhật State hiện tại (base.Update).
    /// 2. Áp dụng trọng lực (ApplyGravity).
    /// 3. Kiểm tra tự động đóng UI khi người chơi di chuyển.
    /// 4. Quét và xử lý tương tác phím F với các vật thể xung quanh.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        ApplyGravity();
        CheckAutoCloseUI();
        HandleInteraction();
    }

    /// <summary>
    /// Mục đích: Tính toán và áp dụng trọng lực lên CharacterController để nhân vật rơi xuống đất mượt mà.
    /// </summary>
    private void ApplyGravity()
    {
        if (characterController.isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f;
        }
        else
        {
            currentVelocity.y -= gravity * Time.deltaTime;
        }
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Mục đích: Kiểm tra nếu người chơi bắt đầu nhấn phím di chuyển mà đang có cửa sổ UI mở,
    /// hệ thống sẽ tự động đóng tất cả UI lại và khóa chuột về tâm màn hình để tiếp tục chơi.
    /// </summary>
    private void CheckAutoCloseUI()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if ((moveInput.x != 0 || moveInput.y != 0) && CursorManager.Instance != null && CursorManager.Instance.IsAnyUIOpen())
        {
            CursorManager.Instance.CloseAllUI();
        }
    }

    /// <summary>
    /// Mục đích: Quét các Collider xung quanh trong bán kính interactionRange để tìm kiếm các đối tượng
    /// triển khai interface IInteractable (NPC, Rương, Cổng Dungeon...) và lắng nghe phím F / Interact để kích hoạt.
    /// </summary>
    private void HandleInteraction()
    {
        nearbyInteractable = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);

        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                nearbyInteractable = interactable;
                break;
            }
        }

        // Kích hoạt tương tác nếu người chơi nhấn phím Interact hoặc F
        if ((inputActions.Player.Interact.triggered || Input.GetKeyDown(KeyCode.F)) && nearbyInteractable != null)
        {
            nearbyInteractable.Interact();
        }
    }

    /// <summary>
    /// Mục đích: Hỗ trợ dịch chuyển (teleport) nhân vật một cách an toàn bằng cách tạm thời tắt CharacterController
    /// để tránh việc va chạm vật lý ghi đè lại vị trí mới.
    /// </summary>
    /// <param name="position">Tọa độ đích cần dịch chuyển đến</param>
    public void Teleport(Vector3 position)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
    }


    /// <summary>
    /// Mục đích: Kiểm tra cờ lưu trong PlayerPrefs xem người chơi có vị trí cần trả về (sau khi thoát Dungeon) hay không,
    /// nếu có thì dịch chuyển người chơi về vị trí đó và xóa cờ.
    /// </summary>
    private void CheckReturnPositionFromDungeon()
    {
        if (PlayerPrefs.GetInt("HasReturnPos", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("ReturnPosX");
            float y = PlayerPrefs.GetFloat("ReturnPosY");
            float z = PlayerPrefs.GetFloat("ReturnPosZ");

            Teleport(new Vector3(x, y, z));

            PlayerPrefs.SetInt("HasReturnPos", 0);
            PlayerPrefs.Save();
        }
    }
}