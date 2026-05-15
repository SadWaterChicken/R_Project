using UnityEngine;

/// <summary>
/// Simple test controller để test pause menu
/// Thêm vào Player hoặc GameManager để test
/// </summary>
public class PauseMenuTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTestLogs = true;
    [SerializeField] private KeyCode testSaveKey = KeyCode.F5;
    [SerializeField] private KeyCode testLoadKey = KeyCode.F9;

    private PauseMenuController pauseMenu;

    private void Start()
    {
        // Tìm PauseMenuController trong scene
        pauseMenu = FindObjectOfType<PauseMenuController>();
        
        if (pauseMenu == null && enableTestLogs)
        {
            Debug.LogWarning("PauseMenuTester: No PauseMenuController found in scene!");
        }
        else if (enableTestLogs)
        {
            Debug.Log("PauseMenuTester: Found PauseMenuController, ready for testing");
        }
    }

    private void Update()
    {
        if (!enableTestLogs) return;

        // Test quick save (F5)
        if (Input.GetKeyDown(testSaveKey))
        {
            TestQuickSave();
        }

        // Test pause status (F9)
        if (Input.GetKeyDown(testLoadKey))
        {
            TestPauseStatus();
        }

        // Test force pause (F1)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestForcePause();
        }
    }

    private async void TestQuickSave()
    {
        Debug.Log("=== TESTING QUICK SAVE ===");
        
        if (GameManager.Instance != null)
        {
            try
            {
                await GameManager.Instance.SaveCurrentGame();
                Debug.Log("✅ Quick save successful!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Quick save failed: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance is null!");
        }
    }

    private void TestPauseStatus()
    {
        Debug.Log("=== TESTING PAUSE STATUS ===");
        
        if (pauseMenu != null)
        {
            bool isPaused = pauseMenu.IsPaused;
            float timeScale = Time.timeScale;
            
            Debug.Log($"Pause Menu Status: {(isPaused ? "PAUSED" : "RUNNING")}");
            Debug.Log($"Time.timeScale: {timeScale}");
            
            if (isPaused && timeScale != 0f)
            {
                Debug.LogWarning("⚠️ Inconsistency: Pause menu shows paused but Time.timeScale != 0");
            }
            else if (!isPaused && timeScale == 0f)
            {
                Debug.LogWarning("⚠️ Inconsistency: Pause menu shows running but Time.timeScale == 0");
            }
            else
            {
                Debug.Log("✅ Pause state is consistent");
            }
        }
        else
        {
            Debug.LogError("❌ PauseMenuController not found!");
        }
    }

    private void TestForcePause()
    {
        Debug.Log("=== TESTING FORCE PAUSE ===");
        
        if (pauseMenu != null)
        {
            pauseMenu.ForcePause();
            Debug.Log("✅ Force pause triggered");
        }
        else
        {
            Debug.LogError("❌ PauseMenuController not found!");
        }
    }

    private void OnGUI()
    {
        if (!enableTestLogs) return;

        // Display test instructions
        GUI.Box(new Rect(10, 10, 300, 120), "Pause Menu Test Controls");
        GUI.Label(new Rect(20, 35, 280, 20), "ESC - Toggle Pause Menu");
        GUI.Label(new Rect(20, 55, 280, 20), "F1 - Force Pause");
        GUI.Label(new Rect(20, 75, 280, 20), "F5 - Test Quick Save");
        GUI.Label(new Rect(20, 95, 280, 20), "F9 - Check Pause Status");
        
        // Display current status
        if (pauseMenu != null)
        {
            string status = pauseMenu.IsPaused ? "PAUSED" : "RUNNING";
            GUI.Box(new Rect(10, 140, 150, 30), $"Status: {status}");
        }
        
        GUI.Box(new Rect(10, 180, 150, 30), $"TimeScale: {Time.timeScale:F1}");
    }
}