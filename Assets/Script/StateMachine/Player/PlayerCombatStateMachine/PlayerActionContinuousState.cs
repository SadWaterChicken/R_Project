using System;
using UnityEngine;

/// <summary>
/// State xử lý hành vi tấn công liên tục (Continuous / Channeling Action) - ví dụ như phun lửa liên tục,
/// vận khí công hoặc đâm liên hoàn trong suốt thời gian người chơi nhấn giữ phím.
/// </summary>
public class PlayerActionContinuousState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    private float tickRate = 0.25f; // Tần suất gây sát thương / vung đòn (giây)
    private float lastTickTime = 0f;
    private bool isStopped = false;

    public PlayerActionContinuousState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerActionContinuousState(PlayerCombatStateMachine.PlayerState key) : base(key) { }

    /// <summary>
    /// Mục đích: Bắt đầu hành vi tấn công liên tục: đánh dấu đang tấn công, kích hoạt hoạt ảnh
    /// lặp lại (continuousAttack) và thực hiện ngay đợt kích hoạt đầu tiên.
    /// </summary>
    public override void EnterState()
    {
        isStopped = false;
        _context.IsAttacking = true;
        lastTickTime = Time.time;

        if (_context.PlayerAnimator != null)
        {
            _context.PlayerAnimator.SetBool("continuousAttack", true);
        }

        TriggerContinuousTick();
    }

    /// <summary>
    /// Mục đích: Dọn dẹp trạng thái khi kết thúc tấn công liên tục: tắt boolean hoạt ảnh trên Animator.
    /// </summary>
    public override void ExitState()
    {
        if (_context.PlayerAnimator != null)
        {
            _context.PlayerAnimator.SetBool("continuousAttack", false);
        }
    }

    /// <summary>
    /// Mục đích: Lặp lại đòn đánh theo nhịp tickRate miễn là người chơi vẫn giữ phím tấn công.
    /// Nếu người chơi thả phím ra thì đánh dấu dừng để chuẩn bị chuyển State.
    /// </summary>
    public override void UpdateState()
    {
        bool isHolding = false;
        if (_context.ActiveSlot == EquipSlot.MainHand)
        {
            isHolding = _context.inputActions.Player.Equipment1.IsPressed();
        }
        else
        {
            isHolding = _context.inputActions.Player.Equipment2.IsPressed();
        }

        if (!isHolding)
        {
            isStopped = true;
            return;
        }

        if (Time.time - lastTickTime >= tickRate)
        {
            lastTickTime = Time.time;
            TriggerContinuousTick();
        }
    }

    /// <summary>
    /// Mục đích: Kích hoạt một nhịp tấn công của vũ khí tương ứng trong chuỗi liên hoàn.
    /// </summary>
    private void TriggerContinuousTick()
    {
        if (_context.ActiveSlot == EquipSlot.MainHand)
        {
            _context.equipmentManager.TriggerMainHandAttack("continuousTick");
        }
        else
        {
            _context.equipmentManager.TriggerOffHandAttack("continuousTick");
        }
    }

    /// <summary>
    /// Mục đích: Kiểm tra nếu người chơi đã nhả phím tấn công thì chuyển sang State ActionRecovery hoặc CombatIdle.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        if (isStopped)
        {
            return PlayerCombatStateMachine.PlayerState.ActionRecovery;
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
