using System.IO;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShopTrigger : MonoBehaviour, IInteractable
{
    // Provide a path relative to StreamingAssets (examples below).
    // Acceptable values:
    // - "MageShop" -> resolves to "MageShop.json"
    // - "Shops/MageShop" -> resolves to "Shops/MageShop.json"
    // - "Shops/MageShop.json" -> used as-is
    public string shopJsonFile; // relative to StreamingAssets

    public string shopName = "Shop";
    public ShopManager shopManager;
    public GameObject interactHint; // small world-space "Press F to Interact" text

    private ShopDataJson cachedShop = null;
    private bool preloadAttempted = false;

    private void Start()
    {
        if (interactHint != null) interactHint.SetActive(false);
        // Try to preload the shop JSON so opening is instant on first press (desktop/editor)
        if (!string.IsNullOrWhiteSpace(shopJsonFile) && shopManager != null && !preloadAttempted)
        {
            preloadAttempted = true;
            var normalized = NormalizeStreamingPath(shopJsonFile);
            var fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, normalized);
            if (!Application.isMobilePlatform && File.Exists(fullPath))
            {
                try
                {
                    var json = File.ReadAllText(fullPath);
                    var shop = JsonUtility.FromJson<ShopDataJson>(json);
                    if (shop != null)
                    {
                        foreach(var item in shop.items) {
                            if (item.BaseData == null) {
                                var _ = item.BaseData;
                            }
                        }
                        cachedShop = shop;
                        Debug.Log("ShopTrigger: Preloaded shop JSON synchronously at Start.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"ShopTrigger: Failed to preload shop JSON synchronously: {e.Message}");
                }
            }
            else
            {
                // On platforms like Android we need the coroutine loader
                StartCoroutine(StreamingAssetsLoader.LoadJsonFromStreamingAssets(
                    normalized,
                    json =>
                    {
                        var shop = JsonUtility.FromJson<ShopDataJson>(json);
                        if (shop != null)
                        {
                            foreach(var item in shop.items) {
                                if (item.BaseData == null) {
                                    var _ = item.BaseData;
                                }
                            }
                            cachedShop = shop;
                            Debug.Log("ShopTrigger: Preloaded shop JSON via coroutine at Start.");
                        }
                    },
                    err => Debug.LogWarning($"ShopTrigger: Preload failed: {err}")
                ));
            }
        }
    }

#if UNITY_EDITOR
    // Editor-time validation to help you set correct paths for each NPC
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(shopJsonFile)) return;

        var normalized = NormalizeStreamingPath(shopJsonFile);
        var fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, normalized);
        if (!File.Exists(fullPath))
            Debug.LogWarning($"[ShopTrigger] StreamingAssets file not found: {fullPath}. Place JSON in Assets/StreamingAssets and set path relative to it (e.g. \"Shops/MageShop.json\").", this);
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactHint != null) interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactHint != null) interactHint.SetActive(false);
            shopManager?.CloseShop();
        }
    }

    /// <summary>
    /// Called by PlayerController when player presses Interact key while in range
    /// </summary>
    public void Interact()
    {
        Debug.Log("ShopTrigger: Interact called by PlayerController");

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

        // If we've preloaded the shop, open it immediately
        if (cachedShop != null)
        {
            Debug.Log("ShopTrigger: Opening preloaded shop immediately.");
            shopManager.OpenShop(cachedShop);
            return;
        }

        var normalized = NormalizeStreamingPath(shopJsonFile);

        // Try synchronous read if the file exists (desktop/editor). This is a fallback if preload didn't run.
        var fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, normalized);
        if (!Application.isMobilePlatform && File.Exists(fullPath))
        {
            try
            {
                var json = File.ReadAllText(fullPath);
                var shop = JsonUtility.FromJson<ShopDataJson>(json);
                if (shop != null)
                {
                    foreach(var item in shop.items) {
                        if (item.BaseData == null) {
                            var _ = item.BaseData;
                        }
                    }
                    Debug.Log("ShopTrigger: Loaded shop JSON synchronously and opening shop.");
                    shopManager.OpenShop(shop);
                }
                else
                {
                    Debug.LogWarning("Failed to parse shop JSON from StreamingAssets (sync read).");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to read shop JSON synchronously: {e.Message}");
            }
            return;
        }

        // Fallback: load via coroutine (needed on some platforms like Android)
        StartCoroutine(StreamingAssetsLoader.LoadJsonFromStreamingAssets(
            normalized,
            json =>
            {
                var shop = JsonUtility.FromJson<ShopDataJson>(json);
                if (shop != null) 
                {
                    foreach(var item in shop.items) {
                        if (item.BaseData == null) {
                            var _ = item.BaseData;
                        }
                    }
                    shopManager.OpenShop(shop);
                }
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
