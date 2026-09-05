using UnityEngine;

/// <summary>
/// State xử lý khi người chơi bị ngắt chiêu (Interrupted / Staggered / Stunned) do bị quái vật đánh trúng,
/// khống chế hoặc mất thăng bằng trong lúc đang ra đòn hay đang phòng thủ.
/// </summary>
public class PlayerCombatInterruptedState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    private float interruptDuration = 0.5f;
    private float enterTime;

    public PlayerCombatInterruptedState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerCombatInterruptedState(PlayerCombatStateMachine.PlayerState key) : base(key) { }

    /// <summary>
    /// Mục đích: Ngắt toàn bộ hành vi tấn công hiện tại, hủy các trigger combo hoạt ảnh,
    /// hạ khiên nếu đang giơ khiên và bắt đầu đếm thời gian bị khống chế/choáng.
    /// </summary>
    public override void EnterState()
    {
        enterTime = Time.time;
        _context.IsAttacking = false;
        _context.ResetAttack();

        if (_context.IsGuarding())
        {
            _context.GuardDown();
        }
    }

    /// <summary>
    /// Mục đích: Dọn dẹp trạng thái khi thoát khỏi trạng thái bị ngắt chiêu.
    /// </summary>
    public override void ExitState()
    {
    }

    /// <summary>
    /// Mục đích: Cập nhật thời gian bị choáng / khống chế.
    /// </summary>
    public override void UpdateState()
    {
    }

    /// <summary>
    /// Mục đích: Kiểm tra nếu đã hết thời gian bị ngắt chiêu (interruptDuration) thì tự động
    /// đưa người chơi trở lại trạng thái CombatIdle để sẵn sàng chiến đấu tiếp.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (Time.time >= enterTime + interruptDuration)
        {
            return PlayerCombatStateMachine.PlayerState.CombatIdle;
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
