using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller cho Pause Menu - Nhấn ESC để hiện/ẩn
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitToTitleButton;

    [Header("Settings Panel (Optional)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmPanel;
    [SerializeField] private TextMeshProUGUI exitConfirmText;
    [SerializeField] private Button exitConfirmButton;
    [SerializeField] private Button exitCancelButton;

    [Header("Auto Save Settings")]
    [SerializeField] private bool autoSaveOnExit = true;
    [SerializeField] private TextMeshProUGUI autoSaveStatusText; // "Auto-saving..."

    private bool isPaused = false;
    private bool isProcessingExit = false;

    private void Start()
    {
        SetupButtons();
        HideAllPanels();
        
        // Đảm bảo game không bị pause ban đầu
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Nhấn ESC để toggle pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isProcessingExit)
            {
                TogglePauseMenu();
            }
        }
    }

    private void SetupButtons()
    {
        // Continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Settings button
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        // Exit to title button
        if (exitToTitleButton != null)
        {
            exitToTitleButton.onClick.AddListener(OnExitToTitleClicked);
        }

        // Settings back button
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
        }

        // Exit confirmation buttons
        if (exitConfirmButton != null)
        {
            exitConfirmButton.onClick.AddListener(OnExitConfirmed);
        }

        if (exitCancelButton != null)
        {
            exitCancelButton.onClick.AddListener(OnExitCancelled);
        }
    }

    /// <summary>
    /// Toggle pause menu on/off
    /// </summary>
    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pause game và hiện menu
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Dừng game time
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Resume game và ẩn menu
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Khôi phục game time

        HideAllPanels();

        Debug.Log("Game Resumed");
    }

    private void HideAllPanels()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
    }

    #region Button Handlers

    private void OnContinueClicked()
    {
        Debug.Log("Continue button clicked");
        ResumeGame();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked");
        
        // Ẩn pause menu, hiện settings
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnSettingsBackClicked()
    {
        Debug.Log("Settings back button clicked");
        
        // Ẩn settings, hiện lại pause menu
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    private void OnExitToTitleClicked()
    {
        Debug.Log("Exit to title button clicked");
        
        // Hiện confirmation dialog
        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(true);
            
            // Update confirmation text
            if (exitConfirmText != null)
            {
                string message = autoSaveOnExit ? 
                    "Auto-save and return to main menu?" : 
                    "Return to main menu?\n(Progress will be lost if not saved)";
                exitConfirmText.text = message;
            }
        }

        // Ẩn pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void OnExitConfirmed()
    {
        Debug.Log("Exit confirmed - Returning to title");
        StartExitProcess();
    }

    private void OnExitCancelled()
    {
        Debug.Log("Exit cancelled");
        
        // Ẩn confirmation, hiện lại pause menu
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);
            
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    #endregion

    #region Exit Process

    private async void StartExitProcess()
    {
        isProcessingExit = true;
        
        // Disable buttons để tránh spam click
        SetButtonsInteractable(false);

        try
        {
            if (autoSaveOnExit)
            {
                await PerformAutoSave();
            }

            // Resume time trước khi chuyển scene
            Time.timeScale = 1f;

            // Chuyển về menu scene
            await LoadMenuScene();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during exit process: {e.Message}");
            
            // Khôi phục UI nếu có lỗi
            SetButtonsInteractable(true);
            isProcessingExit = false;
        }
    }

    private async System.Threading.Tasks.Task PerformAutoSave()
    {
        if (autoSaveStatusText != null)
            autoSaveStatusText.text = "Auto-saving...";

        Debug.Log("Performing auto-save before exit...");

        // Update player position trước khi save
        PlayerData playerData = FindFirstObjectByType<PlayerData>();
        if (playerData != null)
        {
            // Update current position cho auto-save
            string saveLocationId = GetCurrentSavePointId();
            playerData.UpdateCurrentPosition(saveLocationId);
            Debug.Log($"Updated player position before auto-save: {saveLocationId}");
        }
        else
        {
            Debug.LogWarning("No PlayerData found for position update!");
        }
        
        if (GameManager.Instance != null)
        {
            try
            {
                // Save game
                await GameManager.Instance.SaveCurrentGame();
                
                if (autoSaveStatusText != null)
                    autoSaveStatusText.text = "Save complete!";
                    
                Debug.Log("Auto-save completed successfully");
                
                // Delay nhỏ để user thấy message
                await System.Threading.Tasks.Task.Delay(500);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Auto-save failed: {e.Message}");
                
                if (autoSaveStatusText != null)
                    autoSaveStatusText.text = "Save failed!";
                    
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }
    }

    private string GetCurrentSavePointId()
    {
        // Tìm SavePoint gần nhất hoặc dùng default
        SavePoint nearestSavePoint = FindFirstObjectByType<SavePoint>();
        
        if (nearestSavePoint != null)
        {
            return nearestSavePoint.SavePointId;
        }
        
        return "AutoSave_Exit"; // Default save point cho auto-save
    }

    private async System.Threading.Tasks.Task LoadMenuScene()
    {
        if (autoSaveStatusText != null)
            autoSaveStatusText.text = "Returning to menu...";

        Debug.Log("Loading menu scene...");

        // Load menu scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game Menu");
        
        while (!asyncLoad.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        Debug.Log("Menu scene loaded");
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (exitConfirmButton != null)
            exitConfirmButton.interactable = interactable;
            
        if (exitCancelButton != null)
            exitCancelButton.interactable = interactable;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Force pause từ external (ví dụ khi player chết)
    /// </summary>
    public void ForcePause()
    {
        if (!isPaused)
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Check xem game có đang pause không
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// Set auto-save option
    /// </summary>
    public void SetAutoSave(bool enabled)
    {
        autoSaveOnExit = enabled;
    }

    #endregion

    #region Unity Events

    private void OnApplicationPause(bool pauseStatus)
    {
        // Auto pause khi app bị minimize (mobile/console)
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Auto pause khi app mất focus (PC)
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }

    #endregion
}