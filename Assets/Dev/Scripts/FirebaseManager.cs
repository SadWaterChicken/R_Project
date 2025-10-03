using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

/// <summary>
/// Firebase Manager - Quản lý kết nối và lưu/tải dữ liệu từ Firebase Realtime Database
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    private DatabaseReference databaseReference;
    private FirebaseApp firebaseApp;
    private bool isInitialized = false;

    // Events
    public event Action OnFirebaseInitialized;
    public event Action<string> OnFirebaseError;
    public event Action OnSaveCompleted;
    // OnLoadCompleted - Reserved for future use (direct load without GameManager)
    // public event Action<PlayerDataSnapshot> OnLoadCompleted;

    private string currentPlayerId;
    private int currentSaveSlot = 0; // Slot hiện tại được chọn

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseManager: Instance created and marked as DontDestroyOnLoad");
            InitializeFirebase();
        }
        else
        {
            Debug.LogWarning("FirebaseManager: Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                isInitialized = true;
                
                OnFirebaseInitialized?.Invoke();
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                string error = $"Could not resolve Firebase dependencies: {dependencyStatus}";
                Debug.LogError(error);
                OnFirebaseError?.Invoke(error);
            }
        });
    }

    /// <summary>
    /// Set Player ID để sử dụng cho save/load
    /// </summary>
    public void SetPlayerId(string playerId)
    {
        currentPlayerId = playerId;
        Debug.Log($"Player ID set to: {playerId}");
    }

    /// <summary>
    /// Set save slot hiện tại
    /// </summary>
    public void SetCurrentSaveSlot(int slotIndex)
    {
        currentSaveSlot = slotIndex;
        Debug.Log($"Current save slot set to: {slotIndex}");
    }

    /// <summary>
    /// Lưu dữ liệu player lên Firebase (deprecated - dùng SavePlayerDataToSlot)
    /// </summary>
    public async void SavePlayerData(PlayerData playerData)
    {
        await SavePlayerDataToSlot(playerData, currentSaveSlot);
    }

    /// <summary>
    /// Lưu dữ liệu player vào slot được chỉ định
    /// </summary>
    public async System.Threading.Tasks.Task SavePlayerDataToSlot(PlayerData playerData, int slotIndex)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized!");
            return;
        }

        if (string.IsNullOrEmpty(currentPlayerId))
        {
            Debug.LogError("Player ID not set!");
            return;
        }

        try
        {
            PlayerDataSnapshot snapshot = playerData.GetSnapshot();
            SaveSlotData saveSlotData = new SaveSlotData(slotIndex, snapshot);
            
            string json = JsonUtility.ToJson(saveSlotData);
            
            // Lưu vào Firebase theo slot
            await databaseReference
                .Child("players")
                .Child(currentPlayerId)
                .Child("saveSlots")
                .Child($"slot_{slotIndex}")
                .SetRawJsonValueAsync(json);

            OnSaveCompleted?.Invoke();
            Debug.Log($"Player data saved to slot {slotIndex} for {currentPlayerId}");
        }
        catch (Exception e)
        {
            string error = $"Failed to save data to slot {slotIndex}: {e.Message}";
            Debug.LogError(error);
            OnFirebaseError?.Invoke(error);
        }
    }

    /// <summary>
    /// Load dữ liệu player từ Firebase (deprecated - dùng LoadSaveSlot)
    /// </summary>
    public async Task<PlayerDataSnapshot> LoadPlayerData(string playerId = null)
    {
        var saveSlotData = await LoadSaveSlot(playerId ?? currentPlayerId, currentSaveSlot);
        return saveSlotData?.playerDataSnapshot;
    }

    /// <summary>
    /// Load save slot data từ Firebase
    /// </summary>
    public async Task<SaveSlotData> LoadSaveSlot(string playerId, int slotIndex)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized!");
            return null;
        }

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("Player ID not set!");
            return null;
        }

        try
        {
            var dataSnapshot = await databaseReference
                .Child("players")
                .Child(playerId)
                .Child("saveSlots")
                .Child($"slot_{slotIndex}")
                .GetValueAsync();

            if (dataSnapshot.Exists)
            {
                string json = dataSnapshot.GetRawJsonValue();
                SaveSlotData saveSlotData = JsonUtility.FromJson<SaveSlotData>(json);
                
                Debug.Log($"Save slot {slotIndex} loaded successfully for {playerId}");
                return saveSlotData;
            }
            else
            {
                Debug.Log($"No save data found in slot {slotIndex} for {playerId}");
                return null;
            }
        }
        catch (Exception e)
        {
            string error = $"Failed to load slot {slotIndex}: {e.Message}";
            Debug.LogError(error);
            OnFirebaseError?.Invoke(error);
            return null;
        }
    }

    /// <summary>
    /// Kiểm tra xem player có save data không
    /// </summary>
    public async Task<bool> HasSaveData(string playerId = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized!");
            return false;
        }

        string targetPlayerId = playerId ?? currentPlayerId;
        if (string.IsNullOrEmpty(targetPlayerId))
        {
            return false;
        }

        try
        {
            var dataSnapshot = await databaseReference
                .Child("players")
                .Child(targetPlayerId)
                .Child("saveData")
                .GetValueAsync();

            return dataSnapshot.Exists;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check save data: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lấy thời gian save cuối cùng
    /// </summary>
    public async Task<long> GetLastSaveTime(string playerId = null)
    {
        if (!isInitialized)
        {
            return 0;
        }

        string targetPlayerId = playerId ?? currentPlayerId;
        if (string.IsNullOrEmpty(targetPlayerId))
        {
            return 0;
        }

        try
        {
            var dataSnapshot = await databaseReference
                .Child("players")
                .Child(targetPlayerId)
                .Child("lastSaveTime")
                .GetValueAsync();

            if (dataSnapshot.Exists)
            {
                return long.Parse(dataSnapshot.Value.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get last save time: {e.Message}");
        }

        return 0;
    }

    /// <summary>
    /// Xóa save data của player
    /// </summary>
    public async void DeletePlayerData(string playerId = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Firebase not initialized!");
            return;
        }

        string targetPlayerId = playerId ?? currentPlayerId;
        if (string.IsNullOrEmpty(targetPlayerId))
        {
            Debug.LogError("Player ID not set!");
            return;
        }

        try
        {
            await databaseReference
                .Child("players")
                .Child(targetPlayerId)
                .SetValueAsync(null);

            Debug.Log($"Player data deleted for {targetPlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete data: {e.Message}");
        }
    }

    public bool IsInitialized => isInitialized;
}
