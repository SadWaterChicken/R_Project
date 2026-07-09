using UnityEngine;

public class DialogueActivator : MonoBehaviour, INPCInteractable
{
    [SerializeField] private DialogueObject dialogueObject;

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other.TryGetComponent(out DialogueManager manager))
        {
            manager.interactable = this;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out DialogueManager manager))
        {
            if(manager.interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                manager.interactable = null;
            }
        }


    }
    public void InteractPlayer(DialogueManager manager)
    {
        foreach(DialogueResponseEvent responseEvent in GetComponents<DialogueResponseEvent>())
        {
            if (responseEvent.DialogueObject == dialogueObject)
            {
                manager.DialogueUI.AddResponseEvent(responseEvent.Events);
                break;
            }
        }

        manager.DialogueUI.ShowDialogue(dialogueObject);
    }

}
