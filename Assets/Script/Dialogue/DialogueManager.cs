using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public INPCInteractable interactable { get; set; }
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (DialogueUI.IsOpen)
        {
            return;
        }
        if (characterController == null) return;


        if (Input.GetKeyDown(KeyCode.F))
        {
            interactable?.InteractPlayer(this);
        }
    }
}
