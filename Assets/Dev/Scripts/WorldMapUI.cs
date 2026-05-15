using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WorldWaypoint
{
    public string sceneName;
    public string savePointId; // optional
    public string label;
}

public class WorldMapUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private List<WorldWaypoint> waypoints = new List<WorldWaypoint>();
    public GameObject ParentPanel { get; set; }

    private List<GameObject> spawned = new List<GameObject>();

    public void Show()
    {
        Clear();
        foreach (var wp in waypoints)
        {
            var go = Instantiate(buttonPrefab, content);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null) txt.text = string.IsNullOrEmpty(wp.label) ? wp.sceneName : wp.label;
            btn.onClick.AddListener(() => OnWaypointClicked(wp));
            spawned.Add(go);
        }
    }

    private void OnWaypointClicked(WorldWaypoint wp)
    {
        GameManager.Instance.TeleportToScene(wp.sceneName, wp.savePointId);
        SavePoint.CloseAllPanels();
    }

    private void Clear()
    {
        foreach (var s in spawned) Destroy(s);
        spawned.Clear();
    }
}
