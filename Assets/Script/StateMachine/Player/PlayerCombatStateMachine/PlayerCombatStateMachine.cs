using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCombatStateMachine : StateManager<PlayerCombatStateMachine.PlayerState>
{
    public enum PlayerState
    {
        CombatIdle,
        ActionExecute,
        ActionCharge,
        ActionContinuous,
        ActionRecovery,
        CombatInterrupted,
        Defend
    }

    [Header("Component References")]
    public Animator PlayerAnimator;
    public PlayerStat playerStat;
    public InputSystem_Actions inputActions;
    public EquipmentManager equipmentManager;
    public PlayerMovementStateMachine playerMovementStateMachine;

    [Header("Combat Tracking")]
    public EquipSlot ActiveSlot; // Ghi nhớ xem đang dùng tay trái hay tay phải
    public int ComboIndex = 0;  // Số hit combo hiện tại
    public bool IsSecondaryInput;  // Đánh dấu xem là đang bấm chuột trái (false) hay chuột phải (true)
    public float currentSpeed;
    public bool IsAttacking { get; set; } = false;
    public bool IsPrimaryWeapon { get; set; } = false;
    public float lastClickedTime = 0;
    public bool isAiming;
    public float cachedCharge;
    public bool isCharging;
    public float currentCharge;


    [Header("Guard / Defend Settings")]
    public bool isGuarding = false;
    [Tooltip("Tỷ lệ sát thương còn lại khi đỡ đòn thành công (0.5 nghĩa là giảm 50% sát thương)")]
    public float guardDamageReduction = 0.5f;

    [Header("UI & Dialogue References")]
    [Tooltip("Khung hội thoại để kiểm tra chặn đánh khi người chơi đang nói chuyện với NPC")]
    [SerializeField] private DialogueUI dialogueUI = null;
    public DialogueUI DialogueUI => dialogueUI;
    public bool isInterrupted = false;

    /// <summary>
    /// Mục đích: Khởi tạo Input Action và đăng ký toàn bộ các State chiến đấu vào State Manager,
    /// thiết lập trạng thái khởi đầu mặc định là CombatIdle.
    /// </summary>
    void Awake()
    {
        inputActions = new InputSystem_Actions();

        States.Add(PlayerState.CombatIdle, new PlayerCombatIdleState(PlayerState.CombatIdle, this));
        States.Add(PlayerState.ActionExecute, new PlayerActionExecuteState(PlayerState.ActionExecute, this));
        States.Add(PlayerState.ActionCharge, new PlayerActionChargeState(PlayerState.ActionCharge, this));
        States.Add(PlayerState.ActionContinuous, new PlayerActionContinuousState(PlayerState.ActionContinuous, this));
        States.Add(PlayerState.ActionRecovery, new PlayerActionRecoveryState(PlayerState.ActionRecovery, this));
        States.Add(PlayerState.CombatInterrupted, new PlayerCombatInterruptedState(PlayerState.CombatInterrupted, this));
        States.Add(PlayerState.Defend, new PlayerDefendState(PlayerState.Defend, this));

        CurrentState = States[PlayerState.CombatIdle];
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
    /// Mục đích: Vòng lặp cập nhật State Machine chiến đấu hiện tại.
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Mục đích: Animation Event trung gian được gọi từ Frame hoạt ảnh hit1 để kiểm tra
    /// nếu người chơi đã bấm trước lệnh đánh hit2 (Input Buffer) thì chuyển ngay sang hit2 mượt mà.
    /// </summary>
    public void Combohit1Transition()
    {
        if (ComboIndex >= 2)
        {
            PlayerAnimator.SetTrigger("hit2");
        }
    }

    /// <summary>
    /// Mục đích: Đặt lại trạng thái tấn công về ban đầu: xóa combo index, hủy các trigger hoạt ảnh cũ
    /// và thông báo IsAttacking = false để các hệ thống khác (Movement) biết người chơi đã ngừng vung kiếm.
    /// </summary>
    public void ResetAttack()
    {
        if (ComboIndex >= 2 && PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("hit1"))
        {
            return;
        }
        ComboIndex = 0;
        PlayerAnimator.ResetTrigger("hit1");
        PlayerAnimator.ResetTrigger("hit2");
        IsAttacking = false;
    }

    /// <summary>
    /// Mục đích: Nâng khiên/thế thủ: kích hoạt biến boolean hoạt ảnh 'guardUp', bật cờ isGuarding = true,
    /// đồng thời giảm 80% tốc độ di chuyển của nhân vật (chỉ còn 20% tốc độ cơ bản) trong lúc giơ khiên.
    /// </summary>
    public void GuardUp()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetBool("guardUp", true);
        }
        isGuarding = true;

        if (playerStat != null)
        {
            playerStat.movementSpeed *= 0.2f;
        }
    }

    /// <summary>
    /// Mục đích: Hạ khiên/thế thủ: tắt hoạt ảnh 'guardUp', tắt cờ isGuarding = false,
    /// và phục hồi lại 100% tốc độ di chuyển cơ bản cho nhân vật.
    /// </summary>
    public void GuardDown()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetBool("guardUp", false);
        }
        isGuarding = false;

        if (playerStat != null)
        {
            playerStat.movementSpeed /= 0.2f;
        }
    }

    /// <summary>
    /// Mục đích: Trả về trạng thái người chơi có đang giơ khiên/phòng thủ hay không
    /// để hệ thống tính toán sát thương (PlayerStat) áp dụng giảm trừ sát thương.
    /// </summary>
    public bool IsGuarding()
    {
        return isGuarding;
    }

    /// <summary>
    /// Mục đích: Cưỡng chế ngắt trạng thái chiến đấu khi người chơi bị quái vật gây choáng,
    /// hất tung hoặc dính hiệu ứng khống chế nặng.
    /// </summary>
    public void InterruptCombat()
    {
        isInterrupted = true;
    }
}