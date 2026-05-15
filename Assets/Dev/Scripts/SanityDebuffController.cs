using UnityEngine;

/// <summary>
/// Controller để áp dụng debuff khi Sanity tụt xuống các mốc
/// </summary>
public class SanityDebuffController : MonoBehaviour
{
    [Header("Debuff Settings")]
    [SerializeField] private PlayerData playerData;
    
    [Header("Level 1 Debuff (75% Sanity)")]
    [SerializeField] private Color level1_ScreenTint = new Color(0.9f, 0.9f, 0.95f, 1f);
    
    [Header("Level 2 Debuff (50% Sanity)")]
    [SerializeField] private float level2_SpeedReduction = 0.8f; // Giảm tốc độ
    [SerializeField] private float level2_DamageReduction = 0.9f; // Giảm sát thương
    [SerializeField] private Color level2_ScreenTint = new Color(0.8f, 0.8f, 0.9f, 1f);
    
    [Header("Level 3 Debuff (25% Sanity)")]
    [SerializeField] private float level3_SpeedReduction = 0.6f; // Giảm tốc độ nhiều
    [SerializeField] private float level3_DamageReduction = 0.7f; // Giảm sát thương nhiều
    [SerializeField] private int level3_HealthDrainPerSecond = 1; // Mất máu từ từ
    [SerializeField] private Color level3_ScreenTint = new Color(0.7f, 0.7f, 0.85f, 1f);
    [SerializeField] private bool level3_Hallucinations = true; // Ảo giác

    [Header("References")]
    [SerializeField] private SpriteRenderer screenOverlay;
    [SerializeField] private AudioSource heartbeatSound;

    private int currentDebuffLevel = 0;
    private float originalMoveSpeed;
    private int originalAttackPower;
    private float healthDrainTimer = 0f;

    private void Start()
    {
        if (playerData == null)
        {
            playerData = GetComponent<PlayerData>();
        }

        if (playerData != null)
        {
            originalMoveSpeed = playerData.MoveSpeed;
            originalAttackPower = playerData.AttackPower;
            
            // Subscribe to sanity events
            playerData.OnSanityThresholdCrossed += OnSanityThresholdCrossed;
        }
    }

    private void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.OnSanityThresholdCrossed -= OnSanityThresholdCrossed;
        }
    }

    private void Update()
    {
        // Level 3 debuff: health drain
        if (currentDebuffLevel >= 3 && level3_HealthDrainPerSecond > 0)
        {
            healthDrainTimer += Time.deltaTime;
            if (healthDrainTimer >= 1f)
            {
                playerData.TakeDamage(level3_HealthDrainPerSecond);
                healthDrainTimer = 0f;
            }
        }
    }

    private void OnSanityThresholdCrossed(int debuffLevel)
    {
        currentDebuffLevel = debuffLevel;
        ApplyDebuff(debuffLevel);
    }

    private void ApplyDebuff(int level)
    {
        // Remove all debuffs first
        RemoveAllDebuffs();

        switch (level)
        {
            case 0:
                // No debuff
                Debug.Log("Sanity restored! No debuffs active.");
                break;

            case 1:
                // Level 1 debuff
                ApplyLevel1Debuff();
                Debug.Log("Sanity Level 1 Debuff: Slight vision impairment");
                break;

            case 2:
                // Level 2 debuff
                ApplyLevel2Debuff();
                Debug.Log("Sanity Level 2 Debuff: Reduced speed and damage");
                break;

            case 3:
                // Level 3 debuff
                ApplyLevel3Debuff();
                Debug.Log("Sanity Level 3 Debuff: Severe penalties, health drain");
                break;
        }
    }

    private void ApplyLevel1Debuff()
    {
        // Visual effects
        if (screenOverlay != null)
        {
            screenOverlay.color = level1_ScreenTint;
        }

        // TODO: Reduce camera vision/lighting
    }

    private void ApplyLevel2Debuff()
    {
        // Visual effects
        if (screenOverlay != null)
        {
            screenOverlay.color = level2_ScreenTint;
        }

        // Reduce speed
        playerData.MoveSpeed = originalMoveSpeed * level2_SpeedReduction;

        // Reduce damage
        playerData.AttackPower = Mathf.RoundToInt(originalAttackPower * level2_DamageReduction);

        // Audio effects
        if (heartbeatSound != null && !heartbeatSound.isPlaying)
        {
            heartbeatSound.Play();
        }
    }

    private void ApplyLevel3Debuff()
    {
        // Visual effects
        if (screenOverlay != null)
        {
            screenOverlay.color = level3_ScreenTint;
        }

        // Severe speed reduction
        playerData.MoveSpeed = originalMoveSpeed * level3_SpeedReduction;

        // Severe damage reduction
        playerData.AttackPower = Mathf.RoundToInt(originalAttackPower * level3_DamageReduction);

        // Audio effects
        if (heartbeatSound != null)
        {
            heartbeatSound.volume = 0.8f;
            if (!heartbeatSound.isPlaying)
            {
                heartbeatSound.Play();
            }
        }

        // TODO: Implement hallucinations
        if (level3_Hallucinations)
        {
            // Spawn fake enemies, visual distortions, etc.
        }
    }

    private void RemoveAllDebuffs()
    {
        // Reset stats
        if (playerData != null)
        {
            playerData.MoveSpeed = originalMoveSpeed;
            playerData.AttackPower = originalAttackPower;
        }

        // Reset visual
        if (screenOverlay != null)
        {
            screenOverlay.color = Color.white;
        }

        // Stop audio
        if (heartbeatSound != null && heartbeatSound.isPlaying)
        {
            heartbeatSound.Stop();
        }

        healthDrainTimer = 0f;
    }
}
