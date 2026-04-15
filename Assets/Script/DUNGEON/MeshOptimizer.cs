using UnityEngine;

public class MeshOptimizer : MonoBehaviour
{
    public void OptimizeMeshes(GameObject parent)
    {
        // Gộp tất cả child meshes thành 1
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        
        if (meshFilters.Length == 0) return;

        CombineInstance[] combines = new CombineInstance[meshFilters.Length];
        
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combines[i].mesh = meshFilters[i].sharedMesh;
            combines[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combines);
        combinedMesh.name = "CombinedMesh";

        MeshFilter mf = parent.AddComponent<MeshFilter>();
        mf.sharedMesh = combinedMesh;

        MeshCollider mc = parent.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;

        // Xóa tất cả child objects cũ
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child != parent.transform)
                DestroyImmediate(child.gameObject);
        }

        Debug.Log("Meshes optimized!");
    }
}
