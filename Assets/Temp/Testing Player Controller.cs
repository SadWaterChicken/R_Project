using UnityEngine;

public class TestingPlayerController : MonoBehaviour
{
    public float speed = 5f;          // Base movement speed
    public float sprintSpeed = 8f;    // Sprint speed
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;

    public INPCInteractable iNPC {  get;  set; }
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (DialogueUI.IsOpen)
        {
            return;
        }
        if (rb == null) return;

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

        if(Input.GetKeyDown(KeyCode.F))
        {
            iNPC?.InteractPlayer(this);
        }
    }
}
