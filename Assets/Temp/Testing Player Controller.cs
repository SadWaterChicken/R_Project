using UnityEngine;

public class TestingPlayerController : MonoBehaviour
{
    public float speed = 5f;          // Base movement speed
    public float sprintSpeed = 8f;    // Sprint speed

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {


        // Get input axes
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Sprint toggle (Left Shift)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 newPosition = rb.position + move * currentSpeed * Time.deltaTime;

        // Move using Rigidbody
        rb.MovePosition(newPosition);


    }
}
