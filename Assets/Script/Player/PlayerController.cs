using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform cam;
    public PlayerCombat playerCombat;
    public float speed = 6f;
    public float jumpForce = 3.5f;
    public float gravity = -15f;
    public float turnSmoothTime = 0.1f;
    public float dashForce = 15f;
    public float dashCooldown = 0.5f;
    public float rotationSpeed = 5f;
    public float interactionRange = 2f;
    private float targetYRotation = 45f;
    private float verticalVelocity = 0f;
    private float dashCooldownTimer = 0f;

    private InputSystem_Actions inputActions;
    private IInteractable nearbyInteractable;

private void Awake()
    {
        inputActions = new InputSystem_Actions();
        gameObject.tag = "Player";
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
        if (Input.GetMouseButtonDown(0) && playerCombat.IsAttackReady())  // Left mouse button
        {
            playerCombat.Attack();
        }
        // Safety check - if camera is destroyed, return
        if (cam == null)
            return;
        
        // Get input from the new Input System
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;  // Flip when moving left (negative)
        }

        animator.SetFloat("horizontal", Mathf.Abs(horizontal));
        animator.SetFloat("vertical", Mathf.Abs(vertical));

        // Calculate direction RELATIVE TO CAMERA
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        
        Vector3 direction = (camForward * vertical + camRight * horizontal).normalized;
        direction.y = 0f; // Ensure movement is horizontal
        
     
        
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

        // Check for nearby interactables (doors)
        DetectNearbyInteractables();
        
        // Handle interaction (F key)
        if (inputActions.Player.Interact.triggered && nearbyInteractable != null)
        {
            nearbyInteractable.Interact();
        }

        // Camera rotation (Q/E for yaw - left/right)
        if (Input.GetKeyDown(KeyCode.Q)) targetYRotation -= 90f;
        if (Input.GetKeyDown(KeyCode.E)) targetYRotation += 90f;

        // Smoothly rotate the virtual camera
        float currentY = cam.transform.eulerAngles.y;
        float nextY = Mathf.LerpAngle(currentY, targetYRotation, Time.deltaTime * rotationSpeed);

        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, nextY, 0);
    }

    private void DetectNearbyInteractables()
    {
        nearbyInteractable = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);

        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                nearbyInteractable = interactable;
                break;
            }
        }
    }
}
