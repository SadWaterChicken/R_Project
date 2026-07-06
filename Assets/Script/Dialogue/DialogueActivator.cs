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
        if(other.CompareTag("Player") && other.TryGetComponent(out TestingPlayerController test))
        {
            test.iNPC = this;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out TestingPlayerController test))
        {
            if(test.iNPC is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                test.iNPC = null;
            }
        }


    }
    public void InteractPlayer(TestingPlayerController test)
    {
        if(TryGetComponent(out DialogueResponseEvent responseEvents) && responseEvents.DialogueObject == dialogueObject)
        {
            test.DialogueUI.AddResponseEvent(responseEvents.Events);
        }

        test.DialogueUI.ShowDialogue(dialogueObject);
    }
}
