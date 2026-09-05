using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// State chờ thi triển kỹ năng (Skill Idle): liên tục lắng nghe các Action kỹ năng
/// (SkillSlot2 cho tay chính, SkillSlot1 cho tay phụ) và kiểm tra các điều kiện an toàn.
/// </summary>
public class PlayerSkillIdleState : BaseState<PlayerSkillStateMachine.PlayerState>
{
    private PlayerSkillStateMachine _context;

    public PlayerSkillIdleState(PlayerSkillStateMachine.PlayerState key, PlayerSkillStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerSkillIdleState(PlayerSkillStateMachine.PlayerState key) : base(key) { }

    public override void EnterState()
    {
        _context.isExecutingSkill = false;
    }

    public override void ExitState() { }

    public override void UpdateState() { }

    public override PlayerSkillStateMachine.PlayerState GetNextState()
    {
        // 0. Kiểm tra an toàn: Không cho kích hoạt chiêu nếu chuột trỏ vào UI hoặc đang mở hộp thoại
        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool isTalking = _context.DialogueUI != null && _context.DialogueUI.IsOpen;

        if (isPointerOverUI || isTalking)
        {
            return StateKey;
        }

        // 1. Kích hoạt Kỹ năng Vũ khí chính (SkillSlot2 - Phím E)
        if (_context.inputActions.Player.SkillSlot2.WasPressedThisFrame())
        {
            if (_context.CanUseSkill(EquipSlot.MainHand))
            {
                _context.activeSkillSlot = EquipSlot.MainHand;
                return PlayerSkillStateMachine.PlayerState.SkillExecute;
            }
        }

        // 2. Kích hoạt Kỹ năng Vũ khí phụ (SkillSlot1 - Phím Q)
        if (_context.inputActions.Player.SkillSlot1.WasPressedThisFrame())
        {
            if (_context.CanUseSkill(EquipSlot.OffHand))
            {
                _context.activeSkillSlot = EquipSlot.OffHand;
                return PlayerSkillStateMachine.PlayerState.SkillExecute;
            }
        }

        return StateKey;
    }

    public override void OnCollisionEnter(Collision other) { }
    public override void OnCollisionExit(Collision other) { }
    public override void OnCollisionStay(Collision other) { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
}
