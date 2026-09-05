using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombatIdleState : BaseState<PlayerCombatStateMachine.PlayerState>
{
    private PlayerCombatStateMachine _context;
    public PlayerCombatIdleState(PlayerCombatStateMachine.PlayerState key, PlayerCombatStateMachine context) : base(key) => _context = context;
    public PlayerCombatIdleState(PlayerCombatStateMachine.PlayerState key) : base(key) { }



    public override void EnterState()
    {
        Debug.Log("AttackIdleState");
        _context.IsAttacking = false;
    }
    public override void ExitState()
    {

    }
    public override void UpdateState()
    {

    }
    /// <summary>
    /// Mục đích: Xác định State chiến đấu kế tiếp:
    /// 1. Kiểm tra an toàn: Không cho phép ra đòn nếu chuột đang trỏ vào UI hoặc đang trong hộp thoại trò chuyện (Dialogue).
    /// 2. Chuột trái (Equipment1): Đọc ActionType của vũ khí chính (Execute, Charge, Continuous) và chuyển State tương ứng.
    /// 3. Chuột phải (Equipment2): Kiểm tra nếu cầm Khiên (Defend) hoặc tay không thì chuyển sang Defend State;
    ///    nếu cầm vũ khí phụ tấn công thì chuyển sang State tấn công tương ứng.
    /// </summary>
    public override PlayerCombatStateMachine.PlayerState GetNextState()
    {
        if (_context.isInterrupted)
        {
            _context.isInterrupted = false;
            return PlayerCombatStateMachine.PlayerState.CombatInterrupted;
        }
        // 0. Kiểm tra an toàn: Nếu chuột đang tương tác với UI hoặc đang mở khung hội thoại thì không cho ra đòn
        bool isPointerOverUI = UnityEngine.EventSystems.EventSystem.current != null &&
                               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        bool isTalking = _context.DialogueUI != null && _context.DialogueUI.IsOpen;

        if (isPointerOverUI || isTalking)
        {
            return StateKey;
        }

        // 1. Nếu bấm chuột trái (Tấn công vũ khí chính)
        if (_context.inputActions.Player.Equipment1.WasPressedThisFrame())
        {
            _context.ActiveSlot = EquipSlot.MainHand;
            _context.IsSecondaryInput = false;

            var actionType = _context.equipmentManager.GetMainHandActionType();
            if (actionType == CombatActionType.Execute) return PlayerCombatStateMachine.PlayerState.ActionExecute;
            if (actionType == CombatActionType.Charge) return PlayerCombatStateMachine.PlayerState.ActionCharge;
            if (actionType == CombatActionType.Continuous) return PlayerCombatStateMachine.PlayerState.ActionContinuous;
        }

        // 2. Nếu bấm hoặc giữ chuột phải (Vũ khí phụ / Phòng thủ)
        if (_context.inputActions.Player.Equipment2.WasPressedThisFrame() || _context.inputActions.Player.Equipment2.IsPressed())
        {
            _context.ActiveSlot = EquipSlot.OffHand;
            _context.IsSecondaryInput = true;

            // Kiểm tra xem tay trái đang cầm Khiên (Defend) hay là tay không -> Chuyển sang Defend State
            if (_context.equipmentManager == null ||
                !_context.equipmentManager.HasOffHandWeapon() ||
                _context.equipmentManager.GetOffHandCombatStyle() == CombatStyle.Defend)
            {
                return PlayerCombatStateMachine.PlayerState.Defend;
            }

            // Nếu tay trái cầm vũ khí tấn công (Melee, Ranged...) thì thực thi đòn đánh của tay phụ
            if (_context.inputActions.Player.Equipment2.WasPressedThisFrame())
            {
                var actionType = _context.equipmentManager.GetOffHandActionType();
                if (actionType == CombatActionType.Execute) return PlayerCombatStateMachine.PlayerState.ActionExecute;
                if (actionType == CombatActionType.Charge) return PlayerCombatStateMachine.PlayerState.ActionCharge;
                if (actionType == CombatActionType.Continuous) return PlayerCombatStateMachine.PlayerState.ActionContinuous;
            }
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
