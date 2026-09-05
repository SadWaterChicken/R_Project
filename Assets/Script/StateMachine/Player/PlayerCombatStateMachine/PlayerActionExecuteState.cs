using System;
using UnityEngine;

public class PlayerActionExecuteState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    public PlayerActionExecuteState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key) => _context = context;
    public PlayerActionExecuteState(PlayerCombatStateMachine.PlayerState key) : base(key) { }



    public override void EnterState()
    {
        _context.IsAttacking = true;
        _context.lastClickedTime = Time.time;

        _context.ComboIndex++;
        _context.ComboIndex = Mathf.Clamp(_context.ComboIndex, 0, 2);

        // Tự động sinh tên Animation dựa vào số Combo (hit1, hit2...)
        string animTrigger = "hit" + _context.ComboIndex;

        _context.PlayerAnimator.ResetTrigger(_context.ComboIndex == 1 ? "hit2" : "hit1");
        _context.PlayerAnimator.SetTrigger(animTrigger);

        if (_context.ActiveSlot == EquipSlot.MainHand)
        {
            _context.equipmentManager.TriggerMainHandAttack(animTrigger);
        }
        else if (_context.ActiveSlot == EquipSlot.OffHand)
        {
            _context.equipmentManager.TriggerOffHandAttack(animTrigger);
        }
    }
    public override void ExitState()
    {

    }
    public override void UpdateState()
    {
        if (_context.ComboIndex >= 2) return;
        bool isClicking = false;

        //lắng nghe nút bấm để gọi state
        if (_context.inputActions.Player.Equipment1.WasPressedThisFrame())
        {
            isClicking = true;
            _context.IsSecondaryInput = false;
            _context.ActiveSlot = EquipSlot.MainHand;
        }
        else if (_context.inputActions.Player.Equipment2.WasPressedThisFrame())
        {
            isClicking = true;
            _context.IsSecondaryInput = true;
            _context.ActiveSlot = EquipSlot.OffHand;
        }

        if (isClicking)
        {
            _context.lastClickedTime = Time.time;
            _context.ComboIndex++;
            _context.ComboIndex = Mathf.Clamp(_context.ComboIndex, 0, 2);

            string animTrigger = "hit" + _context.ComboIndex;
            _context.PlayerAnimator.SetTrigger(animTrigger);

            if (_context.ActiveSlot == EquipSlot.MainHand)
            {
                _context.equipmentManager.TriggerMainHandAttack(animTrigger);
            }
            else if (_context.ActiveSlot == EquipSlot.OffHand)
            {
                _context.equipmentManager.TriggerOffHandAttack(animTrigger);
            }
        }

    }
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        // Your logic to determine the next state goes here.
        if(_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        // 1. Thoát bình thường khi Animation Event gọi ResetAttack
        if (_context.ComboIndex == 0)
        {
            return PlayerCombatStateMachine.PlayerState.CombatIdle;
        }
        // 2. LƯỚI AN TOÀN: Đề phòng Animation bị kẹt (do di chuyển đứt quãng)
        // Nếu đã trôi qua 1.2 giây kể từ cú click cuối cùng mà vẫn kẹt ở đây thì ép nó về Idle
        if (Time.time - _context.lastClickedTime > 1f)
        {
            _context.ResetAttack(); // Gọi hàm này để tự dọn dẹp biến
            return PlayerCombatStateMachine.PlayerState.CombatIdle;
        }
        return StateKey;
    }


    public override void OnCollisionEnter(Collision other)
    {

    }
    public override void OnCollisionExit(Collision other)
    {

    }
    public override void OnCollisionStay(Collision other)
    {

    }
    public override void OnTriggerEnter(Collider other)
    {

    }
    public override void OnTriggerExit(Collider other)
    {

    }
    public override void OnTriggerStay(Collider other)
    {

    }



}
