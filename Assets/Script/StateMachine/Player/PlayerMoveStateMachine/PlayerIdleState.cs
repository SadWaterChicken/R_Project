using UnityEngine;

public class PlayerIdleState : BaseState<PlayerMovementStateMachine.PlayerState>
{
    private PlayerMovementStateMachine _context;
    public PlayerIdleState(PlayerMovementStateMachine.PlayerState key, PlayerMovementStateMachine context) : base(key) => _context = context;
    public PlayerIdleState(PlayerMovementStateMachine.PlayerState key) : base(key) { }

    public override void EnterState()
    {
        _context.currentVelocity.x = 0f;
        _context.currentVelocity.z = 0f;
    }
    public override void ExitState()
    {

    }
    public override void UpdateState()
    {
        //Kiểm tra vector x và y của player khi về trạng thái idle
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        float horizontal = inputData.x;
        float vertical = inputData.y;
        _context.PlayerAnimator.SetFloat("horizontal", Mathf.Abs(horizontal));
        _context.PlayerAnimator.SetFloat("vertical", Mathf.Abs(vertical));
    }
    public override PlayerMovementStateMachine.PlayerState GetNextState()
    {
        // Your logic to determine the next state goes here.
        if (_context.playerCombatStateMachine != null && _context.playerCombatStateMachine.IsAttacking)
        {
            return StateKey;
        }
        //Di chuyển
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        //Check Vector2 if is moving
        if (inputData.sqrMagnitude > 0.1f)
        {
            return PlayerMovementStateMachine.PlayerState.Walk;
        }
        //Change state to jump
        if (_context.inputActions.Player.Jump.WasPressedThisFrame())
        {
            return PlayerMovementStateMachine.PlayerState.Jump;
        }
        //Change state to sprint
        if (_context.inputActions.Player.Sprint.WasPerformedThisFrame() && inputData.sqrMagnitude > 0.1f)
        {
            return PlayerMovementStateMachine.PlayerState.Sprint;
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
