using UnityEngine;

/// <summary>
/// Waystone: allows player to teleport to another scene's savepoint/position.
/// Press T when in range to trigger cross-scene teleport.
/// </summary>
public class Waystone : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSavePointId; // optional
    [SerializeField] private GameObject interactPrompt;

    private bool playerInRange = false;

    private void Awake()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("Waystone: targetSceneName not set.");
                return;
            }
            GameManager.Instance.TeleportToScene(targetSceneName, targetSavePointId);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }
}
