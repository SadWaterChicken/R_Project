using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform cam;
    public PlayerCombat playerCombat;
    public float jumpForce = 3.5f;
    public float gravity = -15f;
    public float turnSmoothTime = 0.1f;
    public float dashForce = 15f;
    public float dashCooldown = 0.5f;
    public float rotationSpeed = 5f;
    public float interactionRange = 3f; // Increased from 2f to 3f for easier interaction
    public GameObject interactHintUI; // Universal hint UI (e.g. "Press F") that appears over interactables
    private float verticalVelocity = 0f;
    private float dashCooldownTimer = 0f;
    public float dashDuration = 0.2f;
    private bool isDashing = false;
    private Vector3 originalAttackPointPos;

    private InputSystem_Actions inputActions;
    private IInteractable nearbyInteractable;

    private void Start()
    {
        // Store the original attack point position
        originalAttackPointPos = playerCombat.attackPoint.localPosition;

        // Check if we need to restore position after returning from the dungeon
        if (PlayerPrefs.GetInt("HasReturnPos", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("ReturnPosX");
            float y = PlayerPrefs.GetFloat("ReturnPosY");
            float z = PlayerPrefs.GetFloat("ReturnPosZ");
            
            // Teleport Player
            controller.enabled = false; // Disable CharacterController momentarily to allow teleporting
            transform.position = new Vector3(x, y, z);
            controller.enabled = true;
            
            // Clear the flag so they don't teleport every time they restart this scene
            PlayerPrefs.SetInt("HasReturnPos", 0);
            PlayerPrefs.Save();
        }
    }
    private static PlayerController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        // Make the player persistent across scenes so they aren't destroyed when the dungeon loads
        transform.parent = null; // Must be root for DontDestroyOnLoad
        DontDestroyOnLoad(gameObject);

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
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }

    // Helper method to reliably teleport a CharacterController
    public void Teleport(Vector3 position)
    {
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }

    void Update()
    {


        // If the camera was destroyed during a scene transition, automatically find the new Main Camera in this scene
        if (cam == null)
        {
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
                Debug.Log("[PlayerController] Found new Main Camera after scene transition.");
            }
            else
            {
                return; // Wait until a camera is found
            }
        }
            
            // Get input from the new Input System
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            if (horizontal != 0)
            {
                // Ta sẽ lật nguyên cái Scale của GameObject chứa Sprite sẽ lật luôn tất cả các Object con bên trong (như mainHandSocket, offHandSocket, attackPoint)
                float facingDir = horizontal < 0 ? -1f : 1f;
                Vector3 currentScale = spriteRenderer.transform.localScale;
                spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * facingDir, currentScale.y, currentScale.z);

                // Tắt hoàn toàn flipX mặc định để tránh bị lật 2 lần (Double Flip)
                spriteRenderer.flipX = false;
            }

            animator.SetFloat("horizontal", Mathf.Abs(horizontal));
            animator.SetFloat("vertical", Mathf.Abs(vertical));

            // Calculate direction RELATIVE TO CAMERA
            // Use Camera.main to get the actual view rotation, as Cinemachine FreeLook objects do not rotate their own transforms.
            Transform activeCam = Camera.main != null ? Camera.main.transform : cam;
            Vector3 camForward = activeCam.forward;
            Vector3 camRight = activeCam.right;
            
            // Flatten vectors to ensure movement is purely horizontal
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            Vector3 direction = (camForward * vertical + camRight * horizontal).normalized;
            
        
            
            // Handle dash input
            dashCooldownTimer -= Time.deltaTime;
            if (inputActions.Player.Sprint.triggered && dashCooldownTimer <= 0f && !isDashing)
            {
                Vector3 dashDirection = direction;
                if (dashDirection == Vector3.zero)
                {
                    dashDirection = spriteRenderer.flipX ? -camRight : camRight;
                }
                StartCoroutine(DashRoutine(dashDirection.normalized));
            }

            if (!isDashing)
            {
                // Move character
                Vector3 velocity = direction * PlayerStat.Instance.movementSpeed;
                
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
            }

            // Interaction logic
            DetectNearbyInteractables();
            if ((inputActions.Player.Interact.triggered || Input.GetKeyDown(KeyCode.F)) && nearbyInteractable != null)
            {
                nearbyInteractable.Interact();
            }

            // Camera rotation is now handled completely by Cinemachine FreeLook
            // Removed manual Q/E rotation to prevent conflicts and jitter
        }
        

    private void DetectNearbyInteractables()
    {
        nearbyInteractable = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);

        foreach (Collider col in colliders)
        {
            // Use GetComponentInParent to find interactable even if collider is on a child object
            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                nearbyInteractable = interactable;
                break;
            }
        }
    }

    private System.Collections.IEnumerator DashRoutine(Vector3 dashDirection)
    {
        isDashing = true;
        if (PlayerStat.Instance != null) PlayerStat.Instance.isInvincible = true;
        
        // Cooldown uses the stat if available, otherwise fallback to local dashCooldown
        dashCooldownTimer = (PlayerStat.Instance != null) ? PlayerStat.Instance.dashCooldown : dashCooldown;

        float startTime = Time.time;
        
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDirection * dashForce * Time.deltaTime);
            yield return null;
        }

        if (PlayerStat.Instance != null) PlayerStat.Instance.isInvincible = false;
        isDashing = false;
    }
}
