using UnityEngine;

public class DialogueActivator : MonoBehaviour, INPCInteractable
{
    [SerializeField] private DialogueObject dialogueObject;

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
        test.DialogueUI.ShowDialogue(dialogueObject);
    }
}
