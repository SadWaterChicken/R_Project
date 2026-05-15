using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Find all ISaveable instances in the active scene and capture their snapshots
    public SceneSnapshot CaptureActiveScene(string sceneId = null)
    {
        var snap = new SceneSnapshot();
        snap.sceneId = string.IsNullOrEmpty(sceneId) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : sceneId;

        var saveables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var s in saveables)
        {
            snap.entities.Add(s.CaptureState());
        }

        return snap;
    }

    // Restore a snapshot: for each entity, try to find existing object by SaveId and call RestoreState.
    // If not found, we do not instantiate prefabs automatically in this minimal version.
    public void RestoreScene(SceneSnapshot snapshot)
    {
        if (snapshot == null) return;

        var saveables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToList();

        foreach (var ent in snapshot.entities)
        {
            var found = saveables.FirstOrDefault(s => s.SaveId == ent.saveId);
            if (found != null)
            {
                found.RestoreState(ent);
            }
            else
            {
                Debug.LogWarning($"SaveManager: No existing object with SaveId {ent.saveId} to restore. Consider instantiating prefab {ent.prefabKey}.");
            }
        }
    }

    // Utility: serialize scene snapshot to JSON
    public string SerializeSceneSnapshot(SceneSnapshot snap)
    {
        return JsonUtility.ToJson(snap);
    }

    public SceneSnapshot DeserializeSceneSnapshot(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        return JsonUtility.FromJson<SceneSnapshot>(json);
    }
}
