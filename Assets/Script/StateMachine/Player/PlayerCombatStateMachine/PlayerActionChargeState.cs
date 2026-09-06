using System;
using UnityEngine;

/// <summary>
/// State xử lý hành vi gồng đòn / tụ lực (Charge Action) - ví dụ như kéo cung hoặc tụ năng lượng phép thuật.
/// </summary>
public class PlayerActionChargeState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    private float currentCharge = 0f;
    private float maxChargeTime = 1.5f;
    private bool isReleased = false;

    public PlayerActionChargeState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key)
    {
        _context = context;
    }

    public PlayerActionChargeState(PlayerCombatStateMachine.PlayerState key) : base(key) { }

    /// <summary>
    /// Mục đích: Bắt đầu quá trình gồng lực: reset biến đếm thời gian gồng, đánh dấu đang tấn công,
    /// và kích hoạt trigger hoạt ảnh gồng chiêu (Charge).
    /// </summary>
    public override void EnterState()
    {
        currentCharge = 0f;
        isReleased = false;
        _context.IsAttacking = true;

        if (_context.PlayerAnimator != null)
        {
            _context.PlayerAnimator.SetTrigger("charge");
        }
    }

    /// <summary>
    /// Mục đích: Dọn dẹp trạng thái khi kết thúc gồng chiêu.
    /// </summary>
    public override void ExitState()
    {
        currentCharge = 0f;
    }

    /// <summary>
    /// Mục đích: Cập nhật tích lũy lực gồng theo thời gian thực (Time.deltaTime) trong khi người chơi
    /// vẫn đang giữ phím tấn công tương ứng (chuột trái cho MainHand, chuột phải cho OffHand).
    /// </summary>
    public override void UpdateState()
    {
        // Tích lũy mức tụ lực từ 0 đến 1
        currentCharge += Time.deltaTime / maxChargeTime;
        currentCharge = Mathf.Clamp01(currentCharge);

        // Kiểm tra người chơi đã nhả phím tấn công hay chưa
        bool buttonReleased = false;
        if (_context.ActiveSlot == EquipSlot.MainHand)
        {
            buttonReleased = _context.inputActions.Player.Equipment1.WasReleasedThisFrame() ||
                             !_context.inputActions.Player.Equipment1.IsPressed();
        }
        else
        {
            buttonReleased = _context.inputActions.Player.Equipment2.WasReleasedThisFrame() ||
                             !_context.inputActions.Player.Equipment2.IsPressed();
        }

        if (buttonReleased && !isReleased)
        {
            ExecuteChargedAttack();
        }
    }

    /// <summary>
    /// Mục đích: Thực thi đòn tấn công sau khi xả lực gồng: gửi lệnh kích hoạt đòn đánh tới EquipmentManager
    /// và phát hoạt ảnh ra đòn trên Animator.
    /// </summary>
    private void ExecuteChargedAttack()
    {
        isReleased = true;

        if (_context.PlayerAnimator != null)
        {
            _context.PlayerAnimator.ResetTrigger("charge");
            _context.PlayerAnimator.SetTrigger("hit1");
        }

        if (_context.ActiveSlot == EquipSlot.MainHand)
        {
            _context.equipmentManager.TriggerMainHandAttack("hit1");
        }
        else
        {
            _context.equipmentManager.TriggerOffHandAttack("hit1");
        }
    }

    /// <summary>
    /// Mục đích: Kiểm tra điều kiện hoàn thành đòn gồng để chuyển sang State ActionRecovery hồi chiêu
    /// hoặc quay về CombatIdle.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        if (isReleased)
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
