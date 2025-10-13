using UnityEngine;
using TMPro;

/// <summary>
/// Training Dummy - Đối tượng để test combat
/// Hiển thị damage nhận được và có thể reset health
/// </summary>
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool autoReset = true;
    [SerializeField] private float resetDelay = 3f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("UI")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform healthBarTransform;
    [SerializeField] private SpriteRenderer healthBarFill;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;

    private AudioSource audioSource;
    private float resetTimer;
    private bool isFlashing;

    #region Properties

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        SetupHealthBar();
    }

    private void Update()
    {
        // Auto reset
        if (autoReset && currentHealth <= 0)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer >= resetDelay)
            {
                ResetDummy();
            }
        }

        UpdateHealthBar();
    }

    #endregion

    #region IDamageable Implementation

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log($"Dummy took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Visual feedback
        ShowDamageText(damage);
        FlashRed();
        PlaySound(hitSound);

        // Reset timer
        if (currentHealth <= 0)
        {
            resetTimer = 0f;
            Debug.Log("Dummy destroyed! Will reset soon...");
        }
    }

    #endregion

    #region Health Management

    public void ResetDummy()
    {
        currentHealth = maxHealth;
        resetTimer = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        Debug.Log("Dummy reset to full health!");
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    #endregion

    #region Visual Feedback

    private void ShowDamageText(int damage)
    {
        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
            GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            TextMeshPro text = damageTextObj.GetComponent<TextMeshPro>();
            if (text != null)
            {
                text.text = damage.ToString();
                text.color = Color.red;
            }

            // Animate and destroy
            StartCoroutine(AnimateDamageText(damageTextObj));
        }
    }

    private System.Collections.IEnumerator AnimateDamageText(GameObject textObj)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Move up
            textObj.transform.position = startPos + Vector3.up * progress * 2f;

            // Fade out
            TextMeshPro text = textObj.GetComponent<TextMeshPro>();
            if (text != null)
            {
                Color color = text.color;
                color.a = 1f - progress;
                text.color = color;
            }

            yield return null;
        }

        Destroy(textObj);
    }

    private void FlashRed()
    {
        if (!isFlashing && spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        isFlashing = true;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = normalColor;
        isFlashing = false;
    }

    #endregion

    #region Health Bar

    private void SetupHealthBar()
    {
        if (healthBarTransform == null)
        {
            // Create health bar if not assigned
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(transform);
            healthBarObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            healthBarTransform = healthBarObj.transform;

            // Create background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(healthBarTransform);
            bg.transform.localPosition = Vector3.zero;
            SpriteRenderer bgSprite = bg.AddComponent<SpriteRenderer>();
            bgSprite.color = Color.black;
            bgSprite.sprite = CreateSimpleSprite();
            bgSprite.transform.localScale = new Vector3(1.2f, 0.2f, 1f);

            // Create fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(healthBarTransform);
            fill.transform.localPosition = Vector3.zero;
            healthBarFill = fill.AddComponent<SpriteRenderer>();
            healthBarFill.color = Color.green;
            healthBarFill.sprite = CreateSimpleSprite();
            healthBarFill.transform.localScale = new Vector3(1f, 0.15f, 1f);
            healthBarFill.sortingOrder = 1;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            Vector3 scale = healthBarFill.transform.localScale;
            scale.x = healthPercent;
            healthBarFill.transform.localScale = scale;

            // Change color based on health
            if (healthPercent > 0.5f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.25f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }

    private Sprite CreateSimpleSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }

    #endregion
}
