using UnityEngine;
using TMPro;

/// <summary>
/// Debug tool để kiểm tra save/load process và PlayerData changes
/// </summary>
public class SaveDebugger : MonoBehaviour
{
    [Header("UI References (Optional)")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showOnScreenDebug = true;
    [SerializeField] private bool logToConsole = true;

    [Header("Debug Controls")]
    [SerializeField] private KeyCode debugInfoKey = KeyCode.F3;
    [SerializeField] private KeyCode forceSaveKey = KeyCode.F6;
    [SerializeField] private KeyCode checkFirebaseKey = KeyCode.F7;

    private PlayerData playerData;
    private string lastDebugInfo = "";

    private void Start()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        if (playerData == null && logToConsole)
        {
            Debug.LogError("SaveDebugger: No PlayerData found in scene!");
        }
    }

    private void Update()
    {
        // Debug info hotkey
        if (Input.GetKeyDown(debugInfoKey))
        {
            ShowDebugInfo();
        }

        // Force save hotkey
        if (Input.GetKeyDown(forceSaveKey))
        {
            ForceSave();
        }

        // Check Firebase hotkey
        if (Input.GetKeyDown(checkFirebaseKey))
        {
            CheckFirebaseConnection();
        }

        // Update debug display
        if (showOnScreenDebug && debugText != null)
        {
            UpdateDebugDisplay();
        }
    }

    private void ShowDebugInfo()
    {
        if (playerData == null)
        {
            Debug.LogError("No PlayerData to debug!");
            return;
        }

        string info = $"=== PLAYER DATA DEBUG ===\n";
        info += $"Player Name: {playerData.GetPlayerName()}\n";
        info += $"Health: {playerData.GetCurrentHealth()}/{playerData.GetMaxHealth()}\n";
        info += $"Mana: {playerData.GetCurrentMana()}/{playerData.GetMaxMana()}\n";
        info += $"Sanity: {playerData.GetCurrentSanity()}/{playerData.GetMaxSanity()}\n";
        info += $"Gold: {playerData.GetGold()}\n";
        info += $"Gems: {playerData.GetGems()}\n";
        info += $"Last Save Point: {playerData.GetLastSavePointId()}\n";
        info += $"Last Position: {playerData.GetLastSavePosition()}\n";
        info += $"Current Position: {playerData.transform.position}\n";

        // GameManager info
        if (GameManager.Instance != null)
        {
            info += $"Current Save Slot: {GameManager.Instance.GetCurrentSaveSlot()}\n";
        }

        // Firebase info
        if (FirebaseManager.Instance != null)
        {
            info += $"Firebase Initialized: {FirebaseManager.Instance.IsInitialized}\n";
        }

        info += $"========================";

        if (logToConsole)
        {
            Debug.Log(info);
        }

        lastDebugInfo = info;
    }

    private async void ForceSave()
    {
        Debug.Log("=== FORCE SAVE TEST ===");

        if (playerData == null)
        {
            Debug.LogError("No PlayerData to save!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("No GameManager found!");
            return;
        }

        try
        {
            // Update position before save
            playerData.UpdateCurrentPosition("Debug_ForceSave");
            
            Debug.Log("Calling GameManager.SaveCurrentGame()...");
            await GameManager.Instance.SaveCurrentGame();
            Debug.Log("✅ Force save completed successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Force save failed: {e.Message}");
        }
    }

    private void CheckFirebaseConnection()
    {
        Debug.Log("=== FIREBASE CONNECTION CHECK ===");

        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("❌ FirebaseManager.Instance is null!");
            return;
        }

        bool isInitialized = FirebaseManager.Instance.IsInitialized;
        Debug.Log($"Firebase Initialized: {(isInitialized ? "✅ YES" : "❌ NO")}");

        if (GameManager.Instance != null)
        {
            int currentSlot = GameManager.Instance.GetCurrentSaveSlot();
            Debug.Log($"Current Save Slot: {currentSlot}");

            if (currentSlot < 0)
            {
                Debug.LogWarning("⚠️ No save slot selected! This might cause save failures.");
            }
        }

        Debug.Log("=================================");
    }

    private void UpdateDebugDisplay()
    {
        if (debugText == null || playerData == null) return;

        string display = $"Player Stats Debug:\n";
        display += $"HP: {playerData.GetCurrentHealth()}/{playerData.GetMaxHealth()}\n";
        display += $"MP: {playerData.GetCurrentMana()}/{playerData.GetMaxMana()}\n";
        display += $"Sanity: {playerData.GetCurrentSanity()}/{playerData.GetMaxSanity()}\n";
        display += $"Gold: {playerData.GetGold()}\n";
        display += $"Position: {playerData.transform.position}\n";

        if (GameManager.Instance != null)
        {
            display += $"Save Slot: {GameManager.Instance.GetCurrentSaveSlot()}\n";
        }

        display += $"\nControls:\n";
        display += $"F3 - Full Debug Info\n";
        display += $"F6 - Force Save\n";
        display += $"F7 - Check Firebase";

        debugText.text = display;
    }

    /// <summary>
    /// Test method để thay đổi player stats
    /// </summary>
    [ContextMenu("Test Change Player Stats")]
    public void TestChangePlayerStats()
    {
        if (playerData == null)
        {
            Debug.LogError("No PlayerData found!");
            return;
        }

        // Thay đổi một số stats để test
        playerData.AddGold(100);
        playerData.AddGems(10);
        playerData.TakeDamage(20);

        Debug.Log("Changed player stats: +100 gold, +10 gems, -20 health");
        Debug.Log("Use F6 to force save these changes!");
    }

    /// <summary>
    /// Kiểm tra xem có thay đổi gì trong PlayerData không
    /// </summary>
    [ContextMenu("Check For Unsaved Changes")]
    public void CheckForUnsavedChanges()
    {
        if (playerData == null) return;

        Debug.Log("=== CHECKING FOR UNSAVED CHANGES ===");
        Debug.Log("Current stats might be different from last save.");
        Debug.Log("Use SavePoint (Press E) or Force Save (F6) to save changes.");
        Debug.Log("Or use Exit to Title from pause menu for auto-save.");
    }

    private void OnGUI()
    {
        if (!showOnScreenDebug || debugText != null) return;

        // Fallback GUI nếu không có TextMeshPro
        GUI.Box(new Rect(Screen.width - 320, 10, 300, 200), lastDebugInfo);
    }
}