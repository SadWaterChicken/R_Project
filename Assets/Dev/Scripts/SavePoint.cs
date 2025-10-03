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
    [SerializeField] private GameObject interactPrompt; // UI hiển thị "Press E to Save"
    [SerializeField] private ParticleSystem saveEffect; // Effect khi save
    [SerializeField] private AudioClip saveSound; // Âm thanh khi save

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
        if (audioSource == null && saveSound != null)
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
        // Kiểm tra input để save
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SaveGame();
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

    private void SaveGame()
    {
        if (playerData != null)
        {
            Vector3 savePos = spawnPosition != null ? spawnPosition.position : transform.position;
            
            // Sử dụng Save Point
            playerData.UseSavePoint(savePointId, savePos);

            // Lưu vào Firebase
            FirebaseManager.Instance?.SavePlayerData(playerData);

            // Visual & Audio feedback
            if (saveEffect != null)
            {
                saveEffect.Play();
            }

            if (audioSource != null && saveSound != null)
            {
                audioSource.PlayOneShot(saveSound);
            }

            Debug.Log($"Game Saved at {savePointId}");
        }
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
