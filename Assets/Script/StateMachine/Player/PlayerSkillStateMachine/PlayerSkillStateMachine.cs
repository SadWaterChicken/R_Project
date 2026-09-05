using UnityEngine;

/// <summary>
/// State Machine quản lý việc thi triển kỹ năng (Skills) của người chơi:
/// hỗ trợ kỹ năng của vũ khí tay chính (SkillSlot2 - E) và kỹ năng vũ khí tay phụ (SkillSlot1 - Q).
/// </summary>
public class PlayerSkillStateMachine : StateManager<PlayerSkillStateMachine.PlayerState>
{
    public enum PlayerState
    {
        SkillIdle,
        SkillExecute,
        SkillCooldown
    }

    [Header("Component References")]
    public Animator PlayerAnimator;
    public EquipmentManager equipmentManager;
    public PlayerStat playerStat;
    public InputSystem_Actions inputActions;

    [Header("UI & Dialogue References")]
    [Tooltip("Khung hội thoại để kiểm tra chặn thi triển chiêu khi đang nói chuyện với NPC")]
    [SerializeField] private DialogueUI dialogueUI = null;
    public DialogueUI DialogueUI => dialogueUI;

    [Header("Skill State Tracking")]
    public EquipSlot activeSkillSlot = EquipSlot.MainHand;
    public bool isExecutingSkill = false;
    public float skillDuration = 0.5f;

    [Header("Cooldown Settings (Seconds)")]
    public float mainHandCooldown = 2f;
    public float offHandCooldown = 2f;
    public float lastMainHandSkillTime = -999f;
    public float lastOffHandSkillTime = -999f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        States.Add(PlayerState.SkillIdle, new PlayerSkillIdleState(PlayerState.SkillIdle, this));
        States.Add(PlayerState.SkillExecute, new PlayerSkillExecuteState(PlayerState.SkillExecute, this));
        States.Add(PlayerState.SkillCooldown, new PlayerSkillCooldownState(PlayerState.SkillCooldown, this));

        CurrentState = States[PlayerState.SkillIdle];
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    protected override void Update()
    {
        base.Update();
    }

    public bool CanUseSkill(EquipSlot slot)
    {
        if (slot == EquipSlot.MainHand)
        {
            return Time.time >= lastMainHandSkillTime + mainHandCooldown;
        }
        else
        {
            return equipmentManager != null &&

                   equipmentManager.HasOffHandWeapon() &&

                   Time.time >= lastOffHandSkillTime + offHandCooldown;
        }
    }

    public void TriggerSkill(EquipSlot slot)
    {
        if (equipmentManager == null) return;

        if (slot == EquipSlot.MainHand)
        {
            lastMainHandSkillTime = Time.time;
            equipmentManager.TriggerMainHandSkill();
        }
        else
        {
            lastOffHandSkillTime = Time.time;
            equipmentManager.TriggerOffHandSkill();
        }
    }

    public float GetCooldownNormalized(EquipSlot slot)
    {
        float lastTime = slot == EquipSlot.MainHand ? lastMainHandSkillTime : lastOffHandSkillTime;
        float cd = slot == EquipSlot.MainHand ? mainHandCooldown : offHandCooldown;

        float elapsed = Time.time - lastTime;
        if (elapsed >= cd) return 0f;
        return 1f - Mathf.Clamp01(elapsed / cd);
    }
}
