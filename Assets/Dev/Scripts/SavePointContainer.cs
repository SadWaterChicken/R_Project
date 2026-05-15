using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SavePointContainer holds a set of savepoint entries (local positions + spawn offsets)
/// and can instantiate them from a SavePoint prefab. Use the editor helper to capture
/// SavePoint objects into a container and save as a prefab.
/// </summary>
public class SavePointContainer : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string id;
        public Vector3 localPosition;
        public bool hasSpawn;
        public Vector3 spawnLocalPosition;
    }

    [Header("Runtime")]
    [Tooltip("Prefab to instantiate for each entry (must contain SavePoint component)")]
    public GameObject savePointPrefab;

    [Tooltip("If true, ApplyToScene will create instances at Awake automatically")]
    public bool instantiateOnAwake = false;

    [SerializeField]
    public List<Entry> entries = new List<Entry>();

    private void Awake()
    {
        if (instantiateOnAwake)
        {
            ApplyToScene();
        }
    }

    /// <summary>
    /// Instantiate savepoint prefab instances into the scene using the container's transform
    /// as the parent/anchor for local positions.
    /// </summary>
    public void ApplyToScene(Transform parent = null)
    {
        if (savePointPrefab == null)
        {
            Debug.LogError("SavePointContainer: savePointPrefab is not assigned.");
            return;
        }

        Transform containerTransform = this.transform;
        foreach (var e in entries)
        {
            Vector3 worldPos = containerTransform.TransformPoint(e.localPosition);
            GameObject go = Instantiate(savePointPrefab, worldPos, Quaternion.identity);
            if (parent != null) go.transform.SetParent(parent, true);

            var sp = go.GetComponent<SavePoint>();
            if (sp != null)
            {
                if (!string.IsNullOrEmpty(e.id)) sp.SetSavePointId(e.id);

                if (e.hasSpawn)
                {
                    // create or set spawn child
                    Transform spawn = sp.transform.Find("SpawnPosition");
                    if (spawn == null)
                    {
                        var spawnObj = new GameObject("SpawnPosition");
                        spawnObj.transform.SetParent(sp.transform, false);
                        spawn = spawnObj.transform;
                    }
                    spawn.localPosition = e.spawnLocalPosition;
                }
            }
            else
            {
                Debug.LogWarning("SavePointContainer: instantiated prefab does not have SavePoint component.");
            }
        }
    }

    /// <summary>
    /// Clear existing entries (editor helper)
    /// </summary>
    public void ClearEntries()
    {
        entries.Clear();
    }
}
