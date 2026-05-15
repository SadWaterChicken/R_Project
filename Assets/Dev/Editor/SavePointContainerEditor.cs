#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Editor helper to capture selected SavePoint GameObjects into a SavePointContainer
[CustomEditor(typeof(SavePointContainer))]
public class SavePointContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SavePointContainer container = (SavePointContainer)target;

        GUILayout.Space(8);

        if (GUILayout.Button("Capture Selected SavePoints"))
        {
            CaptureSelected(container);
            EditorUtility.SetDirty(container);
        }

        if (GUILayout.Button("Apply To Scene Now"))
        {
            container.ApplyToScene();
        }

        if (GUILayout.Button("Save Container As Prefab"))
        {
            SaveAsPrefab(container);
        }
    }

    private void CaptureSelected(SavePointContainer container)
    {
        var selection = Selection.gameObjects;
        container.ClearEntries();
        Transform t = container.transform;

        foreach (var go in selection)
        {
            var sp = go.GetComponent<SavePoint>();
            if (sp == null) continue;
            var e = new SavePointContainer.Entry();
            e.id = sp.GetSavePointId();
            e.localPosition = t.InverseTransformPoint(go.transform.position);

            var spawn = go.transform.Find("SpawnPosition");
            if (spawn != null)
            {
                e.hasSpawn = true;
                e.spawnLocalPosition = t.InverseTransformPoint(spawn.position);
            }
            container.entries.Add(e);
        }

        Debug.Log($"Captured {container.entries.Count} savepoint(s) into container '{container.name}'");
    }

    private void SaveAsPrefab(SavePointContainer container)
    {
        string dir = "Assets/Dev/Prefabs/SavePointContainers";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets/Dev/Prefabs", "SavePointContainers");
        }

        string path = AssetDatabase.GenerateUniqueAssetPath(dir + "/" + container.name + ".prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(container.gameObject, path);
        if (prefab != null) Debug.Log($"Saved SavePointContainer prefab to {path}");
        else Debug.LogError("Failed to save SavePointContainer prefab");
    }
}
#endif
