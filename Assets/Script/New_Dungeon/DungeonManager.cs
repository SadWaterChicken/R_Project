using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    public int floorCount = 0;

    [Header("UI Feedback")]
    private bool showBossOptions = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnBossDefeated()
    {
        Debug.Log("DungeonManager received Boss Defeated event!");
        // TODO: Give player a reward item here (e.g. spawn a chest)
        
        // Show options to go deeper or escape
        showBossOptions = true;
    }

    public void GoDeeper()
    {
        floorCount++;
        showBossOptions = false;
        Debug.Log($"Going deeper! Current floor: {floorCount}");
        // Reload current scene to reset the dungeon
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EscapeDungeon()
    {
        floorCount = 0;
        showBossOptions = false;
        Debug.Log("Escaped the dungeon! Floor count reset.");
        // Reload current scene or load a hub/town scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Temporary basic UI for Boss Options (To be replaced with actual UI canvas later)
    private void OnGUI()
    {
        if (showBossOptions)
        {
            // Center the menu
            float width = 300;
            float height = 200;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            GUI.Box(new Rect(x, y, width, height), "Boss Defeated! Choose your path:");

            // Give Reward Placeholder
            GUI.Label(new Rect(x + 20, y + 40, width - 40, 30), "You found a powerful artifact!");

            if (GUI.Button(new Rect(x + 50, y + 80, 200, 40), $"Go Deeper (Floor {floorCount + 1})"))
            {
                GoDeeper();
            }

            if (GUI.Button(new Rect(x + 50, y + 130, 200, 40), "Escape Dungeon"))
            {
                EscapeDungeon();
            }
        }
    }
}
