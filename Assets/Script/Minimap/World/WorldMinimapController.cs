using UnityEngine;
using UnityEngine.UI;

public class WorldMinimapController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera minimapCamera;
    public RectTransform playerArrow;

    [Header("Settings")]
    public float cameraHeight = 50f;

    void LateUpdate()
    {
        if (player == null) return;

        // Follow player
        Vector3 pos = player.position;

        minimapCamera.transform.position =
            new Vector3(
                pos.x,
                pos.y + cameraHeight,
                pos.z
            );

        // Camera nhìn xuống
        minimapCamera.transform.rotation =
            Quaternion.Euler(90f, 0f, 0f);

        // Rotate arrow theo player
        playerArrow.localEulerAngles =
            new Vector3(
                0,
                0,
                -player.eulerAngles.y
            );
    }
}
