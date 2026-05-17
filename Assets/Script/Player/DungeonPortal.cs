using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal to enter a dungeon instance. Located on the world map.
/// Implements IInteractable, triggered with F key.
/// Each portal can be customized with a specific dungeon theme (or random).
/// </summary>
public class DungeonPortal : MonoBehaviour, IInteractable
{
   
    
    public string portalName = "Dungeon Portal";
    public GameObject interactHint;
    
    public int entranceID;
    public string dungeonSceneName = "DungeonTesting";

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactHint != null)
                interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    /// <summary>
    /// Called by PlayerController when player presses F near portal.
    /// Takes the player to the dungeon scene.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"[DungeonPortal] Interacting with {portalName} - Loading dungeon scene: {dungeonSceneName}");
        SceneManager.LoadScene(dungeonSceneName);
    }


}
