using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed = 6f;
    public float jumpForce = 3.5f;
    public float gravity = -15f;
    public float turnSmoothTime = 0.1f;
    public float dashForce = 15f;
    public float dashCooldown = 0.5f;
    public float rotationSpeed = 5f;
    private float targetYRotation = 45f;
    
    private float verticalVelocity = 0f;
    private float turnSmoothTimeVelocity;
    private float dashCooldownTimer = 0f;
    
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    void Update()
    {
        // Get input from the new Input System
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        // Calculate direction RELATIVE TO CAMERA
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        
        Vector3 direction = (camForward * vertical + camRight * horizontal).normalized;
        direction.y = 0f; // Ensure movement is horizontal
        
        // Rotate character to face movement direction
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothTimeVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        
        // Move character
        Vector3 velocity = direction * speed;
        
        // Handle dash
        dashCooldownTimer -= Time.deltaTime;
        if (inputActions.Player.Sprint.triggered && dashCooldownTimer <= 0f)
        {
            controller.Move(transform.forward * dashForce * Time.deltaTime);
            dashCooldownTimer = dashCooldown;
        }
        
        // Handle jumping
        if (controller.isGrounded)
        {
            verticalVelocity = 0f;
            if (inputActions.Player.Jump.triggered)
            {
                verticalVelocity = jumpForce;
            }
        }
        
        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        velocity.y = verticalVelocity;
        
        // Move controller
        controller.Move(velocity * Time.deltaTime);

        // Camera rotation (you can add these to InputSystem_Actions if needed)
         if (Input.GetKeyDown(KeyCode.Q)) targetYRotation -= 90f;
         if (Input.GetKeyDown(KeyCode.E)) targetYRotation += 90f;

    // Smoothly rotate the virtual camera
    float currentY = cam.transform.eulerAngles.y;
    float nextY = Mathf.LerpAngle(currentY, targetYRotation, Time.deltaTime * rotationSpeed);
    cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, nextY, 0);
    }
}
