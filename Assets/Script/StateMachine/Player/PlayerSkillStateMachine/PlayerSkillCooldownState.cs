using UnityEngine;

/// <summary>
/// State hồi chiêu kỹ năng (Skill Cooldown): xử lý khoảng thời gian trễ sau khi dùng chiêu (Post-cast recovery)
/// trước khi trả hệ thống kỹ năng về trạng thái SkillIdle.
/// </summary>
public class PlayerSkillCooldownState : BaseState<PlayerSkillStateMachine.PlayerState>
{
    private PlayerSkillStateMachine _context;
    private float postCastDelay = 0.2f;
    private float enterTime;

    public PlayerSkillCooldownState(PlayerSkillStateMachine.PlayerState key, PlayerSkillStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerSkillCooldownState(PlayerSkillStateMachine.PlayerState key) : base(key) { }

    public override void EnterState()
    {
        enterTime = Time.time;
    }

    public override void ExitState() { }

    public override void UpdateState() { }

    public override PlayerSkillStateMachine.PlayerState GetNextState()
    {
        if (Time.time >= enterTime + postCastDelay)
        {
            return PlayerSkillStateMachine.PlayerState.SkillIdle;
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
