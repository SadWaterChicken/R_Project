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
    [SerializeField] private Button playButton;
    // Removed: newGameButton and continueButton - using Save Slot Selection instead
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject loadingPanel;

    [Header("Save Slot Selection")]
    [SerializeField] private SaveSlotSelectionController saveSlotController;

    // Deprecated - không dùng nữa với save slot system
    // [Header("Settings")]
    // [SerializeField] private bool autoCheckSaveData = false;
    // private bool hasSaveData = false;

    private void Start()
    {
        SetupButtons();

        // Không cần check save data cũ nữa vì dùng Save Slot Selection
        // if (autoCheckSaveData)
        // {
        //     CheckForSaveData();
        // }

        // Subscribe to Firebase events (optional)
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseInitialized += OnFirebaseReady;
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseInitialized -= OnFirebaseReady;
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
    }

    private void OnFirebaseReady()
    {
        Debug.Log("Firebase ready for Save Slot Selection!");
        // Không cần check save data cũ nữa
    }

    private void OnPlayClicked()
    {
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
