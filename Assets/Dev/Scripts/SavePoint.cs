using UnityEngine;

/// <summary>
/// Save Point component - Khi player chạm vào sẽ restore đầy và lưu game
/// </summary>
public class SavePoint : MonoBehaviour
{
    [SerializeField] private string savePointId; // ID duy nhất cho save point
    
    // Public property để access từ bên ngoài
    public string SavePointId => savePointId;
    [SerializeField] private Transform spawnPosition; // Vị trí respawn
    [SerializeField] private GameObject interactPrompt; // UI hiển thị "Press F to Rest"
    [SerializeField] private ParticleSystem restEffect; // Effect khi rest (hồi full)
    [SerializeField] private AudioClip restSound; // Âm thanh khi rest

    private PlayerData playerData;
    private AudioSource audioSource;
    private bool playerInRange = false;

    private void Awake()
    {
        // Tự động generate ID nếu chưa có
        if (string.IsNullOrEmpty(savePointId))
        {
            savePointId = "SavePoint_" + transform.position.ToString();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && restSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        // F - Rest at Save Point (save + hồi full HP/Mana/Sanity)
        if (Input.GetKeyDown(KeyCode.F))
        {
            RestAtSavePoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerData = collision.GetComponent<PlayerData>();
            if (playerData != null)
            {
                playerInRange = true;
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Rest at Save Point - Save game VÀ hồi đầy HP/Mana/Sanity (như bonfire trong Dark Souls)
    /// </summary>
    private void RestAtSavePoint()
    {
        if (playerData == null) return;

        Vector3 savePos = spawnPosition != null ? spawnPosition.position : transform.position;
        
        // Sử dụng Save Point - Hồi đầy tất cả stats
        playerData.UseSavePoint(savePointId, savePos);

        // Lưu vào Firebase
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SavePlayerData(playerData);
        }

        // Visual & Audio feedback
        if (restEffect != null)
        {
            restEffect.Play();
        }

        if (audioSource != null && restSound != null)
        {
            audioSource.PlayOneShot(restSound);
        }

        Debug.Log($"[SavePoint] Rested at {savePointId} - Game saved + Fully restored!");
    }

    private void OnDrawGizmosSelected()
    {
        // Hiển thị vị trí spawn trong Editor
        Gizmos.color = Color.green;
        Vector3 spawnPos = spawnPosition != null ? spawnPosition.position : transform.position;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 2);
    }
}
