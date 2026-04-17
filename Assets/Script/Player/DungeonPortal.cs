using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPortal : MonoBehaviour, IInteractable
{
    public int entranceID;
    public string dungeonSceneName = "DungeonTesting";
    
    public void Interact()
    {
        // Pass the entrance ID to the dungeon manager before loading scene
        DungeonSessionManager.currentEntranceID = entranceID;
        SceneManager.LoadScene(dungeonSceneName);
    }
}
