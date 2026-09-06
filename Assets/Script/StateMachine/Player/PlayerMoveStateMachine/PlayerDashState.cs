using UnityEngine;

public class PlayerDashState : BaseState<PlayerMovementStateMachine.PlayerState>
{
    private PlayerMovementStateMachine _context;
    public PlayerDashState(PlayerMovementStateMachine.PlayerState key, PlayerMovementStateMachine context) : base(key) => _context = context;
    public PlayerDashState(PlayerMovementStateMachine.PlayerState key) : base(key) { }

    // Các biến cần thiết cho lướt
    private float dashDuration = 0.2f;
    private float dashForce = 15f;
    private float startTime;
    private Vector3 dashDirection;



    public override void EnterState()
    {
        //Invincible
        if (_context.playerStat != null) _context.playerStat.isInvincible = true;
        startTime = Time.time;
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        dashDirection = (camForward * inputData.y) + (camRight * inputData.x);

        if (dashDirection == Vector3.zero)
        {
            dashDirection = _context.transform.forward;
        }
    }
    public override void ExitState()
    {
        if (_context.playerStat != null) _context.playerStat.isInvincible = false;
    }
    public override void UpdateState()
    {
        _context.characterController.Move(dashDirection * dashForce * Time.deltaTime);
    }
    public override PlayerMovementStateMachine.PlayerState GetNextState()
    {
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        if (Time.time >= startTime + dashDuration)
        {
            if (inputData.sqrMagnitude > 0.1f)
            {
                if (_context.inputActions.Player.Sprint.IsInProgress())
                {
                    return PlayerMovementStateMachine.PlayerState.Sprint;
                }
                return PlayerMovementStateMachine.PlayerState.Walk;
            }
            return PlayerMovementStateMachine.PlayerState.Idle;
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
