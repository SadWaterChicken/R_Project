using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI to list all SavePoints in the current scene and allow the player to teleport to any of them.
/// Requires: a vertical Content Transform and a Button prefab (button should have a child Text component).
/// </summary>
public class SavePointMapUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject buttonPrefab; // prefab with Button component
    public GameObject ParentPanel { get; set; }

    private List<GameObject> spawnedButtons = new List<GameObject>();

    public void Show()
    {
        Clear();

        foreach (var sp in SavePoint.AllSavePoints)
        {
            var go = Instantiate(buttonPrefab, content);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = sp.SavePointId;
            btn.onClick.AddListener(() => OnSavePointClicked(sp));
            spawnedButtons.Add(go);
        }
    }

    private void OnSavePointClicked(SavePoint sp)
    {
        var player = FindFirstObjectByType<PlayerData>();
        if (player != null)
        {
            player.transform.position = sp.SpawnPosition != null ? sp.SpawnPosition.position : sp.transform.position;
            // close panels
            SavePoint.CloseAllPanels();
        }
    }

    private void Clear()
    {
        foreach (var b in spawnedButtons) Destroy(b);
        spawnedButtons.Clear();
    }
}
