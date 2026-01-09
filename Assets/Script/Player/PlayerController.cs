using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float rollForce = 20f;
    [SerializeField] private float rollCooldown = 0.5f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float cameraAngle = 45f; // Isometric camera angle
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float horizontalInput;
    private float verticalInput;
    private float lastRollTime = -1f;
    private bool facingRight = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        GetInput();
        SpeedControl();
        Flip();
    }
    
    void FixedUpdate()
    {
        Move();
    }
    
    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        // Roll/Dash input
        if (Input.GetKeyDown(KeyCode.Space) && CanRoll())
        {
            Roll();
        }
    }
    
    private void Move()
    {
        // Isometric movement: convert WASD to world space
        // A/D for left/right
        // W/S for forward/backward at isometric angle
        float angleRad = cameraAngle * Mathf.Deg2Rad;
        
        float moveX = horizontalInput;
        float moveZ = verticalInput * Mathf.Cos(angleRad);
        
        moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        
        // Determine current speed
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        // Apply movement
        if (moveDirection.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        
        // Apply drag
        rb.linearDamping = groundDrag;
    }
    
    private void SpeedControl()
    {
        // Limit speed on ground
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        float currentMaxSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
    
    private bool CanRoll()
    {
        return Time.time - lastRollTime >= rollCooldown;
    }
    
    private void Roll()
    {
        // Roll in the direction the player is moving or facing
        Vector3 rollDirection = (moveDirection.magnitude > 0.1f) ? moveDirection : (facingRight ? Vector3.right : Vector3.left);
        rollDirection.y = 0;
        rollDirection = rollDirection.normalized;
        
        // Apply roll force
        rb.linearVelocity = rollDirection * rollForce;
        lastRollTime = Time.time;
    }
    
    private void Flip()
    {
        // Flip if moving left and facing right, or moving right and facing left
        if (horizontalInput > 0 && !facingRight)
        {
            FlipPlayer();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            FlipPlayer();
        }
    }
    
    private void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
