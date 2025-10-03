using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controller cho Save Slot Selection Panel
/// </summary>
public class SaveSlotSelectionController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject saveSlotSelectionPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Button backButton;
    
    [Header("Save Slots (3 slots)")]
    [SerializeField] private Button saveSlot1Button;
    [SerializeField] private TextMeshProUGUI saveSlot1Text;
    
    [SerializeField] private Button saveSlot2Button;
    [SerializeField] private TextMeshProUGUI saveSlot2Text;
    
    [SerializeField] private Button saveSlot3Button;
    [SerializeField] private TextMeshProUGUI saveSlot3Text;
    
    [Header("Creating Save Panel")]
    [SerializeField] private GameObject createSavePanel;
    [SerializeField] private TextMeshProUGUI createSaveText;
    [SerializeField] private Slider createSaveProgressBar;

    private SaveSlotData[] currentSaveSlots = new SaveSlotData[3];
    private bool isProcessing = false;

    private void Start()
    {
        SetupButtons();
        
        // Setup 3 save slot buttons
        if (saveSlot1Button != null)
            saveSlot1Button.onClick.AddListener(() => OnSlotClicked(0));
            
        if (saveSlot2Button != null)
            saveSlot2Button.onClick.AddListener(() => OnSlotClicked(1));
            
        if (saveSlot3Button != null)
            saveSlot3Button.onClick.AddListener(() => OnSlotClicked(2));
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSaveSlotsLoaded -= OnSaveSlotsLoaded;
            GameManager.Instance.OnSaveSlotsLoaded += OnSaveSlotsLoaded;
        }

        // Ẩn panel ban đầu
        if (saveSlotSelectionPanel != null)
            saveSlotSelectionPanel.SetActive(false);
        
        if (createSavePanel != null)
            createSavePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSaveSlotsLoaded -= OnSaveSlotsLoaded;
        }
    }

    private void SetupButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    /// <summary>
    /// Hiển thị Save Slot Selection Panel
    /// </summary>
    public void ShowSaveSlotSelection()
    {
        if (isProcessing) return;

        saveSlotSelectionPanel?.SetActive(true);
        ShowLoadingState("Loading save slots...");
        
        // Load tất cả save slots từ Firebase
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadAllSaveSlots();
        }
    }

    private void OnSaveSlotsLoaded(SaveSlotData[] saveSlots)
    {
        Debug.Log($"SaveSlotSelectionController: Received {saveSlots.Length} save slots");
        
        if (saveSlots != null && saveSlots.Length >= 3)
        {
            currentSaveSlots[0] = saveSlots[0];
            currentSaveSlots[1] = saveSlots[1];
            currentSaveSlots[2] = saveSlots[2];
            
            UpdateAllSaveSlotsUI();
        }
        
        HideLoadingState();
    }

    /// <summary>
    /// Update UI cho 3 save slots
    /// </summary>
    private void UpdateAllSaveSlotsUI()
    {
        UpdateSlotUI(0, saveSlot1Text, currentSaveSlots[0]);
        UpdateSlotUI(1, saveSlot2Text, currentSaveSlots[1]);
        UpdateSlotUI(2, saveSlot3Text, currentSaveSlots[2]);
    }

    private void UpdateSlotUI(int slotIndex, TextMeshProUGUI textUI, SaveSlotData slotData)
    {
        if (textUI == null || slotData == null) return;

        if (slotData.isEmpty)
        {
            textUI.text = $"SLOT {slotIndex + 1}\nEmpty";
        }
        else
        {
            textUI.text = $"SLOT {slotIndex + 1}\n{slotData.playerName}\n{FormatTime(slotData.lastSaveTime)}";
        }
        
        Debug.Log($"Updated Slot {slotIndex + 1}: {(slotData.isEmpty ? "Empty" : slotData.playerName)}");
    }

    private string FormatTime(long timestamp)
    {
        if (timestamp == 0) return "Never";
        
        try
        {
            // Firebase sử dụng milliseconds, cần convert sang seconds
            System.DateTime dateTime = System.DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            return dateTime.ToString("MM/dd HH:mm");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to format timestamp {timestamp}: {e.Message}");
            return "Invalid Date";
        }
    }

    /// <summary>
    /// Khi click vào 1 trong 3 save slot buttons
    /// </summary>
    private void OnSlotClicked(int slotIndex)
    {
        if (isProcessing) return;
        if (slotIndex < 0 || slotIndex >= currentSaveSlots.Length) return;

        SaveSlotData selectedSlot = currentSaveSlots[slotIndex];
        
        if (selectedSlot.isEmpty)
        {
            // Tạo save mới
            StartCreateNewSave(slotIndex);
        }
        else
        {
            // Load save có sẵn
            StartLoadExistingSave(selectedSlot);
        }
    }

    private async void StartCreateNewSave(int slotIndex)
    {
        isProcessing = true;
        
        // Hiển thị Creating Save panel
        ShowCreateSavePanel($"Creating new save in Slot {slotIndex + 1}...");
        
        // Simulate progress for better UX
        await SimulateCreateProgress();
        
        // Tạo save mới
        if (GameManager.Instance != null)
        {
            await GameManager.Instance.CreateNewSave(slotIndex);
        }
        
        // Check if this object still exists trước khi hide panels
        // (Scene có thể đã chuyển và object bị destroy)
        if (this != null && gameObject != null)
        {
            HideAllPanels();
            isProcessing = false;
        }
    }

    private async void StartLoadExistingSave(SaveSlotData saveSlotData)
    {
        isProcessing = true;
        
        ShowLoadingState($"Loading {saveSlotData.playerName}...");
        
        // Load save
        if (GameManager.Instance != null)
        {
            await GameManager.Instance.LoadGameFromSlot(saveSlotData.slotIndex);
        }
        
        // Check if this object still exists trước khi hide panels
        // (Scene có thể đã chuyển và object bị destroy)
        if (this != null && gameObject != null)
        {
            HideAllPanels();
            isProcessing = false;
        }
    }

    private void ShowLoadingState(string message)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }

    private void HideLoadingState()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    private void ShowCreateSavePanel(string message)
    {
        if (createSavePanel != null)
        {
            createSavePanel.SetActive(true);
        }
        
        if (createSaveText != null)
        {
            createSaveText.text = message;
        }
        
        if (createSaveProgressBar != null)
        {
            createSaveProgressBar.value = 0f;
        }
    }

    private async System.Threading.Tasks.Task SimulateCreateProgress()
    {
        if (createSaveProgressBar == null) return;

        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.02f; // Tăng 2% mỗi frame
            createSaveProgressBar.value = progress;
            
            // Update text theo progress
            if (createSaveText != null)
            {
                int percentage = Mathf.RoundToInt(progress * 100);
                createSaveText.text = $"Creating new save... {percentage}%";
            }
            
            await System.Threading.Tasks.Task.Delay(50); // 50ms delay
        }
        
        createSaveProgressBar.value = 1f;
        if (createSaveText != null)
        {
            createSaveText.text = "Save created! Starting game...";
        }
        
        await System.Threading.Tasks.Task.Delay(500); // Pause để user đọc
    }

    private void HideAllPanels()
    {
        // Null checks để tránh crash khi scene đã unload
        if (saveSlotSelectionPanel != null && saveSlotSelectionPanel)
            saveSlotSelectionPanel.SetActive(false);
            
        if (loadingPanel != null && loadingPanel)
            loadingPanel.SetActive(false);
            
        if (createSavePanel != null && createSavePanel)
            createSavePanel.SetActive(false);
    }

    private void OnBackClicked()
    {
        HideAllPanels();
        // Không cần clear data vì sử dụng pre-created slots
        isProcessing = false;
    }

    /// <summary>
    /// Public method để gọi từ main menu
    /// </summary>
    public void OnPlayButtonClicked()
    {
        ShowSaveSlotSelection();
    }
}