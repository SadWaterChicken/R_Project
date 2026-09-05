using UnityEngine;

public class PlayerWalkState : BaseState<PlayerMovementStateMachine.PlayerState>
{
    private PlayerMovementStateMachine _context;
    public PlayerWalkState(PlayerMovementStateMachine.PlayerState key, PlayerMovementStateMachine context) : base(key) => _context = context;
    public PlayerWalkState(PlayerMovementStateMachine.PlayerState key) : base(key) { }


    public override void EnterState()
    {

    }
    public override void ExitState()
    {

    }
    public override void UpdateState()
    {
        // 1. Đọc Input bằng New Input System
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        // 2. Lấy hướng Camera
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        _context.currentSpeed = _context.playerStat.movementSpeed;
        // Ép trục Y về 0 để không bị bay lên trời
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        float horizontal = inputData.x;
        float vertical = inputData.y;
        // 3. Tính toán hướng di chuyển mới
        Vector3 movement = (camForward * inputData.y) + (camRight * inputData.x);
        // 4. Xoay nhân vật (chú ý: dùng _context.transform)
        if (movement.magnitude >= 0.1f)
        {
            float facingDir = horizontal < 0 ? -1f : 1f;
            Vector3 currentScale = _context.spriteRenderer.transform.localScale;
            //flip sprite
            _context.spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * facingDir, currentScale.y, currentScale.z);
        }
        // 5. Di chuyển CharacterController
        Vector3 horizontalMove = movement * _context.currentSpeed;

        _context.characterController.Move(horizontalMove * Time.deltaTime);
        // 6. Áp dụng animator
        _context.PlayerAnimator.SetFloat("horizontal", Mathf.Abs(horizontal));
        _context.PlayerAnimator.SetFloat("vertical", Mathf.Abs(vertical));

    }
    public override PlayerMovementStateMachine.PlayerState GetNextState()
    {
        // Your logic to determine the next state goes here.
        if (_context.playerCombatStateMachine != null && _context.playerCombatStateMachine.IsAttacking)
        {
            return PlayerMovementStateMachine.PlayerState.Idle;
        }
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        // Change back to Idle when not moving
        if (inputData.sqrMagnitude < 0.1f)
        {
            return PlayerMovementStateMachine.PlayerState.Idle;
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
