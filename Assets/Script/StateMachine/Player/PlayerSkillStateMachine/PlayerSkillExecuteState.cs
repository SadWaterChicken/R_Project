using UnityEngine;

/// <summary>
/// State thực thi kỹ năng (Skill Execute): kích hoạt hành vi kỹ năng của vũ khí tương ứng,
/// phát hoạt ảnh kỹ năng và duy trì thời gian vận chiêu trước khi chuyển sang hồi chiêu.
/// </summary>
public class PlayerSkillExecuteState : BaseState<PlayerSkillStateMachine.PlayerState>
{
    private PlayerSkillStateMachine _context;
    private float startTime;

    public PlayerSkillExecuteState(PlayerSkillStateMachine.PlayerState key, PlayerSkillStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerSkillExecuteState(PlayerSkillStateMachine.PlayerState key) : base(key) { }

    public override void EnterState()
    {
        startTime = Time.time;
        _context.isExecutingSkill = true;

        if (_context.PlayerAnimator != null)
        {
            _context.PlayerAnimator.SetTrigger("skill");
        }

        _context.TriggerSkill(_context.activeSkillSlot);
    }

    public override void ExitState()
    {
        _context.isExecutingSkill = false;
    }

    public override void UpdateState() { }

    public override PlayerSkillStateMachine.PlayerState GetNextState()
    {
        if (Time.time >= startTime + _context.skillDuration)
        {
            return PlayerSkillStateMachine.PlayerState.SkillCooldown;
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
