using UnityEngine;

/// <summary>
/// State xử lý trạng thái Phòng thủ (Guard / Defend) của người chơi khi cầm khiên hoặc tay không giữ chuột phải.
/// </summary>
public class PlayerDefendState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;

    public PlayerDefendState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerDefendState(PlayerCombatStateMachine.PlayerState key) : base(key) { }

    /// <summary>
    /// Mục đích: Kích hoạt tư thế phòng thủ: bật cờ IsGuarding, giảm tốc độ di chuyển của nhân vật còn 20%,
    /// và kích hoạt hoạt ảnh phòng thủ (guardUp) trên Animator.
    /// </summary>
    public override void EnterState()
    {
        _context.IsAttacking = false;
        _context.GuardUp();
    }

    /// <summary>
    /// Mục đích: Thoát khỏi trạng thái phòng thủ: tắt cờ IsGuarding, khôi phục tốc độ di chuyển bình thường,
    /// và tắt hoạt ảnh guardUp trên Animator.
    /// </summary>
    public override void ExitState()
    {
        _context.GuardDown();
    }

    /// <summary>
    /// Mục đích: Cập nhật trong lúc đang phòng thủ (nếu cần bổ sung logic tiêu hao thể lực/stamina theo thời gian).
    /// </summary>
    public override void UpdateState()
    {
        // Có thể mở rộng trừ thể lực (Stamina) theo thời gian tại đây nếu có hệ thống Stamina
    }

    /// <summary>
    /// Mục đích: Kiểm tra điều kiện chuyển State: nếu người chơi thả nút phòng thủ (Chuột phải / Equipment2),
    /// State Machine sẽ chuyển về trạng thái CombatIdle.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        // Khi người chơi thả nút chuột phải hoặc nút không còn được giữ
        if (_context.inputActions.Player.Equipment2.WasReleasedThisFrame() ||

            !_context.inputActions.Player.Equipment2.IsPressed())
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
