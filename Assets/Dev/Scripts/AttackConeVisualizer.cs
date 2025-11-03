using System.Collections;
using UnityEngine;

/// <summary>
/// Creates a temporary filled triangle mesh representing an attack cone.
/// Call AttackConeVisualizer.ShowCone(origin, direction, angleDeg, range, duration, color)
/// </summary>
public class AttackConeVisualizer : MonoBehaviour
{
    /// <summary>
    /// Show a triangular cone visual at origin pointing along direction.
    /// </summary>
    public static void ShowCone(Vector3 origin, Vector2 direction, float angleDeg, float range, float duration = 0.35f, Color? color = null)
    {
        Color c = color ?? Color.yellow;
        GameObject go = new GameObject("AttackConeVisualizer");
        go.transform.position = origin;
        var av = go.AddComponent<AttackConeVisualizer>();
        av.StartCoroutine(av.PlayCone(direction, angleDeg, range, duration, c));
    }

    private IEnumerator PlayCone(Vector2 direction, float angleDeg, float range, float duration, Color color)
    {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        // Use an unlit transparent material (Sprites/Default works well)
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        mr.material = mat;

        // Build triangle vertices (origin at (0,0,0))
        Vector3 dir = new Vector3(direction.normalized.x, direction.normalized.y, 0f);
        float half = angleDeg * 0.5f;
        Quaternion q1 = Quaternion.Euler(0, 0, half);
        Quaternion q2 = Quaternion.Euler(0, 0, -half);

        Vector3 p1 = Vector3.zero;
        Vector3 p2 = q1 * (dir * range);
        Vector3 p3 = q2 * (dir * range);

        Mesh m = new Mesh();
        m.vertices = new Vector3[] { p1, p2, p3 };
        m.triangles = new int[] { 0, 1, 2 };
        m.RecalculateNormals();
        mf.mesh = m;

        // Face camera (optional) - keep as world-space triangle

        float elapsed = 0f;
        Color start = mat.color;
        // initial alpha
        start.a = Mathf.Clamp01(start.a);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float a = Mathf.Lerp(start.a, 0f, t);
            Color cur = mat.color;
            cur.a = a;
            mat.color = cur;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
