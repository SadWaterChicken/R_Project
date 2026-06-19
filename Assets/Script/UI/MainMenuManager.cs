using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý sự kiện cho giao diện Main Menu (Play, Quit, Restart/Continue)
/// Tương thích với các nút bấm từ gói Free UI build package
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scenes to Load")]
    [Tooltip("Tên của Scene gameplay chính để tải khi nhấn Play")]
    public string gameplaySceneName = "PlayerTesting"; 

    [Header("UI Buttons")]
    [Tooltip("Nút Play để bắt đầu game mới")]
    public Button playButton;
    
    [Tooltip("Nút Quit để thoát game")]
    public Button quitButton;

    [Tooltip("Nút Restart trong asset (chúng ta có thể tận dụng làm nút Continue hoặc Restart tùy chọn)")]
    public Button restartButton;

    [Header("Settings")]
    [Tooltip("Nếu true: Nút Restart sẽ có chức năng Continue (Tải lại dữ liệu đã lưu). Nếu false: Reset màn chơi")]
    public bool useRestartAsContinue = true;

    private void Start()
    {
        // Gán sự kiện Click cho nút Play
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Chưa gán Play Button trong Inspector!");
        }

        // Gán sự kiện Click cho nút Quit
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Chưa gán Quit Button trong Inspector!");
        }

        // Gán sự kiện Click cho nút Restart/Continue
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            
            // Nếu dùng làm nút Continue, chúng ta sẽ chỉ cho phép bấm khi có dữ liệu đã lưu
            if (useRestartAsContinue)
            {
                bool hasSave = PlayerPrefs.HasKey("HasReturnPos") || PlayerPrefs.HasKey("SavedScene");
                restartButton.interactable = hasSave;
            }
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút Play (Chơi mới)
    /// </summary>
    public void OnPlayClicked()
    {
        Debug.Log("[MainMenuManager] Bắt đầu chơi mới...");
        
        // Reset toàn bộ dữ liệu lưu trước đó để bắt đầu game mới
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Tải scene gameplay chính
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Xử lý khi nhấn nút Restart hoặc Continue
    /// </summary>
    public void OnRestartClicked()
    {
        if (useRestartAsContinue)
        {
            Debug.Log("[MainMenuManager] Đang tiếp tục màn chơi cũ...");
            string savedScene = PlayerPrefs.GetString("SavedScene", gameplaySceneName);
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            Debug.Log("[MainMenuManager] Chơi lại màn hiện tại...");
            // Load lại scene gameplay chính mà không dùng dữ liệu đã lưu
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút Quit (Thoát)
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("[MainMenuManager] Thoát game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dừng chạy trong Unity Editor
        #else
        Application.Quit(); // Thoát hẳn ứng dụng khi build
        #endif
    }
}
