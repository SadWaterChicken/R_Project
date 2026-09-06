using UnityEngine;

public class PlayerJumpState : BaseState<PlayerMovementStateMachine.PlayerState>
{
    private PlayerMovementStateMachine _context;
    public PlayerJumpState(PlayerMovementStateMachine.PlayerState key, PlayerMovementStateMachine context) : base(key) => _context = context;
    public PlayerJumpState(PlayerMovementStateMachine.PlayerState key) : base(key) { }

    public override void EnterState()
    {

        _context.currentVelocity.y = 6f;
        //call animation for jump here

        //............................
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

        // Ép trục Y về 0 để không bị bay lên trời
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        // 3. Tính toán hướng di chuyển mới
        Vector3 movement = (camForward * inputData.y) + (camRight * inputData.x);
        float horizontal = inputData.x;
        float vertical = inputData.y;

        // 4. Lật sprite theo hướng nhìn (trái / phải) để đồng bộ với 2.5D billboard sprite
        if (movement.magnitude >= 0.1f && _context.spriteRenderer != null)
        {
            float facingDir = horizontal < 0 ? -1f : 1f;
            Vector3 currentScale = _context.spriteRenderer.transform.localScale;
            _context.spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * facingDir, currentScale.y, currentScale.z);
        }

        // 5. Di chuyển CharacterController
        Vector3 horizontalMove = movement * _context.currentSpeed;
        _context.characterController.Move(horizontalMove * Time.deltaTime);
    }
    public override PlayerMovementStateMachine.PlayerState GetNextState()
    {
        // Your logic to determine the next state goes here.
        Vector2 inputData = _context.inputActions.Player.Move.ReadValue<Vector2>();
        if (_context.currentVelocity.y <= 0 && _context.characterController.isGrounded)
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
