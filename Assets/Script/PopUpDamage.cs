using UnityEngine;
using TMPro;

public class PopUpDamage : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Camera mainCamera;

    [Header("Settings")]
    public float moveYSpeed = 2f;
    public float disappearTimerMax = 0.5f;
    public float fadeSpeed = 3f;
    public Vector3 spawnOffset = new Vector3(0, 1.5f, 0);
    public Vector3 randomizeOffset = new Vector3(0.5f, 0f, 0.5f); // Thêm random để không đè nhau

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        mainCamera = Camera.main;
    }

    public void Setup(int damageAmount)
    {
        // Randomize the offset slightly so text doesn't overlap perfectly
        Vector3 randomJitter = new Vector3(
            Random.Range(-randomizeOffset.x, randomizeOffset.x),
            Random.Range(-randomizeOffset.y, randomizeOffset.y),
            Random.Range(-randomizeOffset.z, randomizeOffset.z)
        );

        // Apply the offset
        transform.position += spawnOffset + randomJitter;

        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
            textColor = textMesh.color;
        }
        disappearTimer = disappearTimerMax;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Face the camera
            transform.rotation = mainCamera.transform.rotation;
        }

        // Jump upward
        transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;

        // Decrease timer
        disappearTimer -= Time.deltaTime;

        // Fade away after the timer (0.5s default)
        if (disappearTimer < 0)
        {
            if (textMesh != null)
            {
                textColor.a -= fadeSpeed * Time.deltaTime;
                textMesh.color = textColor;

                if (textColor.a <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
