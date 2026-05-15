using UnityEngine;

/// <summary>
/// Save Point component - Khi player chạm vào sẽ restore đầy và lưu game
/// </summary>
public class SavePoint : MonoBehaviour
{
    [SerializeField] private string savePointId; // ID duy nhất cho save point
    
    // Public property để access từ bên ngoài
    public string SavePointId => savePointId;
    public Transform SpawnPosition => spawnPosition;
    [SerializeField] private Transform spawnPosition; // Vị trí respawn
    [SerializeField] private GameObject interactPrompt; // UI hiển thị "Press F to Rest"
    [SerializeField] private ParticleSystem restEffect; // Effect khi rest (hồi full)
    [SerializeField] private AudioClip restSound; // Âm thanh khi rest
    [Header("UI Panels")]
    [SerializeField] private GameObject restPanel; // panel shown when pressing F (can be empty)
    [SerializeField] private GameObject mapPanel; // M opens this while rest panel is open
    [SerializeField] private GameObject worldMapPanel; // N opens this while rest panel is open
    [Header("Debug / Visibility")]
    [Tooltip("If enabled the SavePoint will force world-space canvas settings to help debug visibility")]
    [SerializeField] private bool debugForceVisible = false;
    [Tooltip("Sorting order to apply when debugForceVisible is true (higher = on top)")]
    [SerializeField] private int debugSortingOrder = 100;

    private PlayerData playerData;
    private AudioSource audioSource;
    private bool playerInRange = false;
    private static readonly System.Collections.Generic.List<SavePoint> allSavePoints = new System.Collections.Generic.List<SavePoint>();
    public static System.Collections.Generic.IReadOnlyList<SavePoint> AllSavePoints => allSavePoints.AsReadOnly();
    private SavePointMapUI mapUI;
    private WorldMapUI worldMapUI;

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
            if (mapPanel != null)
            {
                mapUI = mapPanel.GetComponent<SavePointMapUI>();
                if (mapUI != null) mapUI.ParentPanel = mapPanel;
            }
            if (worldMapPanel != null)
            {
                worldMapUI = worldMapPanel.GetComponent<WorldMapUI>();
                if (worldMapUI != null) worldMapUI.ParentPanel = worldMapPanel;
            }
        }

        // If interactPrompt wasn't set in the inspector, try to find a child named "InteractPrompt"
        if (interactPrompt == null)
        {
            var child = transform.Find("InteractPrompt");
            if (child != null)
            {
                interactPrompt = child.gameObject;
                Debug.Log($"[SavePoint] Auto-assigned interactPrompt from child for '{savePointId}'");
            }
            else
            {
                // try any child that contains a Canvas or TextMeshPro as a fallback
                var canvasChild = GetComponentInChildren<Canvas>(true);
                if (canvasChild != null)
                {
                    interactPrompt = canvasChild.gameObject;
                    Debug.Log($"[SavePoint] Auto-assigned interactPrompt from Canvas child for '{savePointId}'");
                }
            }
        }

        if (interactPrompt != null)
        {
            // Ensure the prompt is a child of this SavePoint so it stays fixed at the save point
            if (interactPrompt.transform.parent != this.transform)
            {
                interactPrompt.transform.SetParent(this.transform, false);
            }

            // Prefer a world-space Canvas so the prompt sits in world coordinates and
            // doesn't behave like screen-space UI attached to the camera/player.
            var canvas = interactPrompt.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.overrideSorting = true;
                // Put the prompt slightly behind typical player ordering. Adjust if needed.
                canvas.sortingOrder = -1;

                // If debug mode is enabled, force a high sorting order so the prompt is visible
                if (debugForceVisible)
                {
                    canvas.sortingOrder = debugSortingOrder;
                }

                // If no camera assigned, assign main camera so UI can compute screen positions
                if (canvas.worldCamera == null && Camera.main != null)
                {
                    canvas.worldCamera = Camera.main;
                }

                // Ensure a reasonable world-space scale so text is visible in scene units
                // Normalize scale: if it's large or approximately default (1), set a small world-space scale
                if (canvas.transform.localScale.sqrMagnitude > 0.1f)
                {
                    canvas.transform.localScale = Vector3.one * 0.01f;
                }

                // Ensure the prompt sits slightly above the savepoint (adjust if needed)
                var rt = canvas.GetComponent<RectTransform>();
                if (rt != null)
                {
                    if (Mathf.Abs(rt.localPosition.y) < 0.01f)
                    {
                        rt.localPosition = new Vector3(0f, 1.2f, 0f);
                    }
                }
            }

            // Hide via controller (keeps object active) or deactivate if no controller exists
            var promptCtrl = interactPrompt.GetComponent<InteractPromptController>();
            if (promptCtrl != null)
            {
                promptCtrl.HideImmediate();
            }
            else
            {
                interactPrompt.SetActive(false);
            }
        }
        // register
        if (!allSavePoints.Contains(this)) allSavePoints.Add(this);

        if (restPanel != null) restPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (worldMapPanel != null) worldMapPanel.SetActive(false);
        if (mapPanel != null) mapUI = mapPanel.GetComponent<SavePointMapUI>();
        if (worldMapPanel != null) worldMapUI = worldMapPanel.GetComponent<WorldMapUI>();
    }

    private void OnDestroy()
    {
        if (allSavePoints.Contains(this)) allSavePoints.Remove(this);
    }

    private void Update()
    {
        if (!playerInRange) return;

        // F - Rest at Save Point (save + hồi full HP/Mana/Sanity)
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Show rest panel and keep rest functionality
            if (restPanel != null)
            {
                restPanel.SetActive(true);
            }
            RestAtSavePoint();
        }

        // While rest panel is open, support Map (M) and World Map (N)
        if (restPanel != null && restPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (mapPanel != null)
                {
                    bool was = mapPanel.activeSelf;
                    mapPanel.SetActive(!was);
                    if (!was) mapUI?.Show();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                if (worldMapPanel != null)
                {
                    bool was = worldMapPanel.activeSelf;
                    worldMapPanel.SetActive(!was);
                    if (!was) worldMapUI?.Show();
                }
            }
        }
    }

    // Close all rest/map/world panels for all savepoints
    public static void CloseAllPanels()
    {
        foreach (var sp in allSavePoints)
        {
            if (sp.restPanel != null) sp.restPanel.SetActive(false);
            if (sp.mapPanel != null) sp.mapPanel.SetActive(false);
            if (sp.worldMapPanel != null) sp.worldMapPanel.SetActive(false);
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
                    Debug.Log($"[SavePoint] Player entered savepoint '{savePointId}' - interactPrompt assigned? {interactPrompt != null}");
                    if (interactPrompt != null)
                    {
                        var promptCtrl = interactPrompt.GetComponent<InteractPromptController>();
                        if (promptCtrl != null)
                        {
                            Debug.Log("[SavePoint] Using InteractPromptController.Show()");
                            promptCtrl.Show();
                        }
                        else
                        {
                            Debug.Log("[SavePoint] No controller found, SetActive(true)");
                            interactPrompt.SetActive(true);
                        }
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
                var promptCtrl = interactPrompt.GetComponent<InteractPromptController>();
                if (promptCtrl != null)
                {
                    Debug.Log("[SavePoint] Hiding prompt via controller");
                    promptCtrl.Hide();
                }
                else
                {
                    Debug.Log("[SavePoint] Hiding prompt via SetActive(false)");
                    interactPrompt.SetActive(false);
                }
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

    private void TryTeleportToAnotherSavePoint()
    {
        // Teleport removed - map-driven flow will be used instead
    }

    private void OnDrawGizmosSelected()
    {
        // Hiển thị vị trí spawn trong Editor
        Gizmos.color = Color.green;
        Vector3 spawnPos = spawnPosition != null ? spawnPosition.position : transform.position;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 2);
    }

    // Editor/runtime helpers
    public string GetSavePointId()
    {
        return savePointId;
    }

    public void SetSavePointId(string id)
    {
        savePointId = id;
    }
}
