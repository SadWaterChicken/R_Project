using UnityEngine;

/// <summary>
/// SaveOnQuit will attempt to save the player's current position and player data when the application quits.
/// It uses FirebaseManager.SavePlayerData (fire-and-forget) and also calls PlayerData.UpdateCurrentPosition.
/// </summary>
public class SaveOnQuit : MonoBehaviour
{
    private void OnEnable()
    {
        Application.quitting += OnAppQuitting;
    }

    private void OnDisable()
    {
        Application.quitting -= OnAppQuitting;
    }

    private void OnAppQuitting()
    {
        try
        {
            var playerData = FindFirstObjectByType<PlayerData>();
            if (playerData != null)
            {
                // Update saved position to current position
                playerData.UpdateCurrentPosition();

                // Trigger a save via FirebaseManager (async void internal)
                if (FirebaseManager.Instance != null)
                {
                    FirebaseManager.Instance.SavePlayerData(playerData);
                    Debug.Log("SaveOnQuit: Triggered SavePlayerData on quit.");
                }
                else
                {
                    Debug.LogWarning("SaveOnQuit: FirebaseManager not available on quit.");
                }
            }
            else
            {
                Debug.LogWarning("SaveOnQuit: No PlayerData found to save on quit.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveOnQuit: Exception while saving on quit: {e.Message}");
        }
    }
}
