using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Menu Controller - Điều khiển UI Menu
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button playButton;
    // Removed: newGameButton and continueButton - using Save Slot Selection instead
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI welcomeText;

    [Header("Save Slot Selection")]
    [SerializeField] private SaveSlotSelectionController saveSlotController;

    [Header("Authentication")]
    [SerializeField] private bool requireLogin = true; // Bắt buộc đăng nhập trước khi play

    // Deprecated - không dùng nữa với save slot system
    // [Header("Settings")]
    // [SerializeField] private bool autoCheckSaveData = false;
    // private bool hasSaveData = false;

    private void Start()
    {
        SetupButtons();
        CheckAuthenticationStatus();

        // Subscribe to Firebase events (optional)
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseInitialized += OnFirebaseReady;
        }

        // Subscribe to auth events
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnSignOutSuccess += OnUserSignedOut;
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseInitialized -= OnFirebaseReady;
        }

        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnSignOutSuccess -= OnUserSignedOut;
        }
    }

    /// <summary>
    /// Kiểm tra xem user đã đăng nhập chưa
    /// </summary>
    private void CheckAuthenticationStatus()
    {
        if (!requireLogin)
        {
            // Không cần login, hiển thị main menu luôn
            ShowMainMenu();
            return;
        }

        // Nếu require login, kiểm tra auth status
        if (AuthenticationManager.Instance != null && AuthenticationManager.Instance.IsSignedIn)
        {
            // User đã đăng nhập
            ShowMainMenu();
            UpdateWelcomeText();
        }
        else
        {
            // User chưa đăng nhập, hiển thị login screen
            ShowLoginScreen();
        }
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // Removed: newGameButton and continueButton setup
        // Now using Save Slot Selection system

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(OnSignOutClicked);
        }
    }

    #region UI Management

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
            
        if (loginPanel != null)
            loginPanel.SetActive(false);
    }

    private void ShowLoginScreen()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (loginPanel != null)
            loginPanel.SetActive(true);
    }

    private void UpdateWelcomeText()
    {
        if (welcomeText != null && AuthenticationManager.Instance != null)
        {
            string displayName = AuthenticationManager.Instance.UserDisplayName;
            welcomeText.text = $"Welcome, {displayName}!";
        }
    }

    #endregion

    private void OnFirebaseReady()
    {
        Debug.Log("Firebase ready for Save Slot Selection!");
        // Không cần check save data cũ nữa
    }

    private void OnPlayClicked()
    {
        // Kiểm tra authentication nếu cần
        if (requireLogin && AuthenticationManager.Instance != null && !AuthenticationManager.Instance.IsSignedIn)
        {
            Debug.LogWarning("User not signed in! Showing login screen...");
            ShowLoginScreen();
            return;
        }

        if (saveSlotController != null)
        {
            // Sử dụng Save Slot Selection System
            saveSlotController.OnPlayButtonClicked();
        }
        else
        {
            // Fallback: Không còn hỗ trợ auto-play, cần dùng Save Slot Selection
            Debug.LogWarning("Save Slot Selection Controller not assigned! Please assign it in MenuController.");
        }
    }

    private void OnSignOutClicked()
    {
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.SignOut();
        }
    }

    /// <summary>
    /// Called by LoginUIController when user signs in successfully
    /// </summary>
    public void OnUserSignedIn()
    {
        Debug.Log("MenuController: User signed in, showing main menu");
        ShowMainMenu();
        UpdateWelcomeText();

        // Update Firebase player ID
        if (FirebaseManager.Instance != null && AuthenticationManager.Instance != null)
        {
            string userId = AuthenticationManager.Instance.UserId;
            FirebaseManager.Instance.SetPlayerId(userId);
        }
    }

    private void OnUserSignedOut()
    {
        Debug.Log("MenuController: User signed out");
        ShowLoginScreen();
    }

    private void OnNewGameClicked()
    {
        // Deprecated - Dùng Save Slot Selection thay thế
        // Confirm dialog nếu có save data
        // if (hasSaveData)
        // {
        //     // TODO: Show confirm dialog
        //     Debug.Log("Warning: Starting new game will overwrite existing save!");
        // }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
    }

    private void OnContinueClicked()
    {
        // Continue game từ save
        if (GameManager.Instance != null)
        {
            LoadGameAsync();
        }
    }

    private async void LoadGameAsync()
    {
        await GameManager.Instance.LoadGame();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings clicked - TODO: Implement settings menu");
        // TODO: Open settings panel
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
