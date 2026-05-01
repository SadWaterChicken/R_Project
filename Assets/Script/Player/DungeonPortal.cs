using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPortal : MonoBehaviour, IInteractable
{
    public int entranceID;
    public string dungeonSceneName = "DungeonTesting";

    public void Interact()
    {
        // Load the dungeon scene
        SceneManager.LoadScene(dungeonSceneName);
    }
}
