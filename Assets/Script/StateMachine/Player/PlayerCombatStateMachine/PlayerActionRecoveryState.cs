using UnityEngine;

/// <summary>
/// State xử lý giai đoạn hồi phục (Recovery / Cooldown) sau khi kết thúc một chuỗi combo đòn đánh,
/// giúp nhân vật có độ trễ hợp lý trước khi có thể bắt đầu chu kỳ tấn công tiếp theo.
/// </summary>
public class PlayerActionRecoveryState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    private float recoveryDuration = 0.2f;
    private float stateEnterTime;

    public PlayerActionRecoveryState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerActionRecoveryState(PlayerCombatStateMachine.PlayerState key) : base(key) { }

    /// <summary>
    /// Mục đích: Khởi tạo thời gian bước vào trạng thái hồi chiêu, dọn dẹp các trigger đòn đánh cũ
    /// để chuẩn bị cho lượt tấn công tiếp theo.
    /// </summary>
    public override void EnterState()
    {
        stateEnterTime = Time.time;
        _context.IsAttacking = false;
        _context.ResetAttack();
    }

    /// <summary>
    /// Mục đích: Dọn dẹp trạng thái khi kết thúc giai đoạn hồi phục.
    /// </summary>
    public override void ExitState()
    {
    }

    /// <summary>
    /// Mục đích: Cập nhật trong thời gian chờ hồi phục.
    /// </summary>
    public override void UpdateState()
    {
    }

    /// <summary>
    /// Mục đích: Kiểm tra nếu đã hết thời gian hồi phục (recoveryDuration) thì đưa trạng thái chiến đấu
    /// về lại CombatIdle để người chơi có thể tiếp tục ra đòn mới.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        if (Time.time >= stateEnterTime + recoveryDuration)
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
