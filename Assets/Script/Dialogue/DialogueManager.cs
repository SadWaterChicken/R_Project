using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public INPCInteractable interactable { get; set; }
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;
    private CharacterController characterController;
    private Rigidbody rb;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (DialogueUI != null && DialogueUI.IsOpen)
        {
            return;
        }
        if (characterController == null && rb == null) return;


        if (Input.GetKeyDown(KeyCode.F))
        {
            interactable?.InteractPlayer(this);
        }
    }
}
