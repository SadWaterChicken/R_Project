using UnityEngine;

public class DungeonExit : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("[DungeonExit] Player interacting with DungeonExit portal!");

        // Return to the overworld (this automatically handles the loading screen and teleporting)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToOverworld();
        }
        else
        {
            Debug.LogError("[DungeonExit] GameStateManager.Instance is null! Cannot return to overworld.");
        }
    }
}
