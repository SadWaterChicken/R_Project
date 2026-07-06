using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public INPCInteractable interactable { get; set; }
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;
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


        if (Input.GetKeyDown(KeyCode.F))
        {
            interactable?.InteractPlayer(this);
        }
    }
}
