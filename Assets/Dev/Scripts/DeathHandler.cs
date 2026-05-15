using System.Collections;
using UnityEngine;

/// <summary>
/// Subscribes to PlayerData.OnPlayerDeath and auto-revives the player after a short delay.
/// Also disables PlayerMovement/Combat while dead (if present).
/// Attach this to a central object (GameManager or an empty bootstrapper) in the scene.
/// </summary>
public class DeathHandler : MonoBehaviour
{
    [Tooltip("Seconds to wait before reviving the player")]
    public float reviveDelay = 1.0f;

    private PlayerData playerData;

    private void Start()
    {
    // Use FindAnyObjectByType to avoid obsolete API warnings on newer Unity versions
    playerData = UnityEngine.Object.FindAnyObjectByType<PlayerData>();
        if (playerData != null)
        {
            playerData.OnPlayerDeath += OnPlayerDeath;
        }
        else
        {
            Debug.LogWarning("DeathHandler: No PlayerData found in scene to subscribe to");
        }
    }

    private void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    private void OnPlayerDeath()
    {
        StartCoroutine(HandleRevive());
    }

    private IEnumerator HandleRevive()
    {
        // Optionally disable movement/combat
        var movement = playerData.GetComponent<MonoBehaviour>();
        // try to find common components to disable if present
        var pm = playerData.GetComponent<PlayerMovement>();
        var pc = playerData.GetComponent<PlayerCombat>();

        if (pm != null) pm.enabled = false;
        if (pc != null) pc.enabled = false;

        yield return new WaitForSecondsRealtime(reviveDelay);

        // Revive at last savepoint
        playerData.Revive();

        // Re-enable components
        if (pm != null) pm.enabled = true;
        if (pc != null) pc.enabled = true;
    }
}
