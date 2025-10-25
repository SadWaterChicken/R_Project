using System.IO;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopTrigger : MonoBehaviour
{
    // Provide a path relative to StreamingAssets (examples below).
    // Acceptable values:
    // - "MageShop" -> resolves to "MageShop.json"
    // - "Shops/MageShop" -> resolves to "Shops/MageShop.json"
    // - "Shops/MageShop.json" -> used as-is
    public string shopJsonFile; // relative to StreamingAssets

    public string shopName = "Shop";
    public ShopManager shopManager;
    public GameObject interactHint; // small world-space "Press E" text

    private bool playerInRange = false;

    private void Start()
    {
        if (interactHint != null) interactHint.SetActive(false);
    }

#if UNITY_EDITOR
    // Editor-time validation to help you set correct paths for each NPC
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(shopJsonFile)) return;

        var normalized = NormalizeStreamingPath(shopJsonFile);
        var fullPath = Path.Combine(Application.streamingAssetsPath, normalized);
        if (!File.Exists(fullPath))
            Debug.LogWarning($"[ShopTrigger] StreamingAssets file not found: {fullPath}. Place JSON in Assets/StreamingAssets and set path relative to it (e.g. \"Shops/MageShop.json\").", this);
    }
#endif

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactHint != null) interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactHint != null) interactHint.SetActive(false);
            shopManager?.CloseShop();
        }
    }

    private void Update()
    {
        if (!(playerInRange && Input.GetKeyDown(KeyCode.E))) return;

        if (shopManager == null)
        {
            Debug.LogWarning("ShopManager not assigned on ShopTrigger.");
            return;
        }

        if (string.IsNullOrWhiteSpace(shopJsonFile))
        {
            Debug.LogWarning("Shop JSON path not assigned on ShopTrigger.");
            return;
        }

        var normalized = NormalizeStreamingPath(shopJsonFile);

        // Load JSON from StreamingAssets (filename must include extension and be relative to StreamingAssets)
        StartCoroutine(StreamingAssetsLoader.LoadJsonFromStreamingAssets(
            normalized,
            json =>
            {
                var shop = JsonUtility.FromJson<ShopDataJson>(json);
                if (shop != null) shopManager.OpenShop(shop);
                else Debug.LogWarning("Failed to parse shop JSON from StreamingAssets.");
            },
            err => Debug.LogWarning($"Failed to load shop JSON: {err}")
        ));
    }

    private static string NormalizeStreamingPath(string input)
    {
        var s = input.Trim().Replace("\\", "/");
        if (s.StartsWith("/")) s = s.Substring(1);
        if (!s.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
            s = s + ".json";
        return s;
    }
}