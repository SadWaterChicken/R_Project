using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SmearMeshGenerator : WeaponComponent
{
    [Header("Smear Settings")]
    public Transform basePoint;
    public Transform tipPoint;
    public Material smearMaterial;
    public int smoothingSegments = 3; // Số điểm nội suy Bezier giữa 2 frame
    public float trailDuration = 0.3f; // Thời gian vệt chém tồn tại
    public float fadeSpeed = 3f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh smearMesh;
    
    private bool isSwinging = false;
    private List<Vector3> basePoints = new List<Vector3>();
    private List<Vector3> tipPoints = new List<Vector3>();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        smearMesh = new Mesh();
        meshFilter.mesh = smearMesh;
        
        if (smearMaterial == null)
        {
            // Default material if missing
            smearMaterial = new Material(Shader.Find("Sprites/Default"));
            smearMaterial.color = new Color(1f, 1f, 1f, 0.5f);
        }
        meshRenderer.material = smearMaterial;
        meshRenderer.enabled = false;
    }

    public override void Initialize(WeaponController weaponController)
    {
        base.Initialize(weaponController);
        
        // Cố gắng tìm Transform con có tên "Base" và "Tip" do User tự canh chỉnh
        if (basePoint == null)
        {
            Transform b = transform.Find("Base") ?? transform.GetComponentInChildren<Transform>().Find("Base");
            if (b != null) basePoint = b;
            else basePoint = this.transform;
        }

        if (tipPoint == null)
        {
            Transform tip = transform.Find("Tip") ?? transform.GetComponentInChildren<Transform>().Find("Tip");
            if (tip != null) tipPoint = tip;
            else 
            {
                Debug.LogWarning("[SmearMeshGenerator] Vũ khí này chưa có điểm Tip! Vui lòng tạo một GameObject con tên là 'Tip' bên trong Prefab 3D để canh chỉnh chiều dài vệt chém thủ công.");
                tipPoint = this.transform; // Fallback để không văng lỗi đỏ
            }
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "StartSwing")
        {
            StartSwing();
        }
        else if (eventName == "EndSwing")
        {
            EndSwing();
        }
    }

    private void StartSwing()
    {
        if (isSwinging) return;
        isSwinging = true;
        basePoints.Clear();
        tipPoints.Clear();
        smearMesh.Clear();
        
        Color c = meshRenderer.material.color;
        c.a = 0.8f;
        meshRenderer.material.color = c;
        meshRenderer.enabled = true;
        
        StartCoroutine(RecordSwingPoints());
    }

    private void EndSwing()
    {
        isSwinging = false;
        StartCoroutine(FadeOutSmear());
    }

    private IEnumerator RecordSwingPoints()
    {
        while (isSwinging)
        {
            basePoints.Add(basePoint.position);
            tipPoints.Add(tipPoint.position);
            
            if (basePoints.Count > 1)
            {
                GenerateMesh();
            }
            yield return null;
        }
    }

    private IEnumerator FadeOutSmear()
    {
        float alpha = meshRenderer.material.color.a;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            Color c = meshRenderer.material.color;
            c.a = Mathf.Max(alpha, 0);
            meshRenderer.material.color = c;
            yield return null;
        }
        meshRenderer.enabled = false;
        smearMesh.Clear();
    }

    private void GenerateMesh()
    {
        if (basePoints.Count < 2) return;

        // Triangulate a simple quad strip from the recorded points
        Vector3[] vertices = new Vector3[basePoints.Count * 2];
        Vector2[] uv = new Vector2[basePoints.Count * 2];
        int[] triangles = new int[(basePoints.Count - 1) * 6];

        for (int i = 0; i < basePoints.Count; i++)
        {
            vertices[i * 2] = transform.InverseTransformPoint(basePoints[i]);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(tipPoints[i]);

            float u = (float)i / (basePoints.Count - 1);
            uv[i * 2] = new Vector2(u, 0);
            uv[i * 2 + 1] = new Vector2(u, 1);
        }

        int t = 0;
        for (int i = 0; i < basePoints.Count - 1; i++)
        {
            int baseIndex = i * 2;
            
            triangles[t++] = baseIndex;
            triangles[t++] = baseIndex + 1;
            triangles[t++] = baseIndex + 2;

            triangles[t++] = baseIndex + 1;
            triangles[t++] = baseIndex + 3;
            triangles[t++] = baseIndex + 2;
        }

        smearMesh.vertices = vertices;
        smearMesh.uv = uv;
        smearMesh.triangles = triangles;
        smearMesh.RecalculateNormals();
        smearMesh.RecalculateBounds();
    }
}
