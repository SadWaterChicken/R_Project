using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 6f;
    public float jumpForce = 3.5f;
    public float gravity = -15f;
    public float turnSmoothTime = 0.1f;
    public float dashForce = 15f;
    public float dashCooldown = 0.5f;
    
    private float verticalVelocity = 0f;
    private float turnSmoothTimeVelocity;
    private float dashCooldownTimer = 0f;

    void Update()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
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
        if (Input.GetKeyDown(KeyCode.Q) && dashCooldownTimer <= 0f)
        {
            controller.Move(transform.forward * dashForce * Time.deltaTime);
            dashCooldownTimer = dashCooldown;
        }
        
        // Handle jumping
        if (controller.isGrounded)
        {
            verticalVelocity = 0f;
            if (Input.GetButtonDown("Jump"))
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
}
