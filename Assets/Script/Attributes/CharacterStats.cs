using UnityEngine;

public abstract class CharacterStats : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth ;
    public float currentHealth;
    public float healthRegenRate;

    [Header("Mana Stats")]
    public int maxMana;
    public float currentMana;
    public float manaRegenRate;

    [Header("Damage Stats")]
    public float basePhysicalDamage;
    public float physicalDamageBonus;
    [HideInInspector] public float physicalDamageMultiplier;

    public float baseMagicDamage;
    public float magicDamageBonus;
    [HideInInspector] public float magicDamageMultiplier;

    [Header("Defence Stats")]
    public float physicalArmor;
    public float magicArmor;

    [Header("Movement & Combat")]
    public float baseSpeed;
    public float movementSpeed;
    public float attackSpeed;

    [Header("Luck & Crit")]
    [Tooltip("1.0 means 100%")]
    public float critChance;
    public float critDamageMultiplier = 2.0f;

    [Header("Shield Stats")]
    public float shield;
    public float maxShield;
    public float shieldRegenRate;
    public float shieldRechargeCooldown;
    
    protected float shieldRechargeTimer;
    protected float lastDamageTime;

    protected CharacterBuffManager buffManager;

    [Header("Animation")]
    public Animator animator;
    protected bool isDead = false;

    [Header("PopUp Damage")]
    public GameObject damagePopupPrefab; // Assign your TextMeshPro prefab here


    protected virtual void Awake()
    {
        buffManager = GetComponent<CharacterBuffManager>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        shield = 0f;
        shieldRechargeTimer = 0f;
        lastDamageTime = Time.time;
    }

    protected virtual void Update()
    {
        // Shield regeneration logic
        UpdateShieldRegeneration();
    }

    protected void UpdateShieldRegeneration()
    {
        if (shield >= maxShield) return;

        shieldRechargeTimer -= Time.deltaTime;
        if (shieldRechargeTimer <= 0 && shieldRegenRate > 0)
        {
            shield += shieldRegenRate * Time.deltaTime;
            shield = Mathf.Min(shield, maxShield);
        }
    }

    // ─── Stat Getters (With Buff Support) ──────────────────────────────────────
    public virtual float GetPhysicalDamage()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.DamageBoost) : 0f;
        return (basePhysicalDamage + physicalDamageBonus) * (1f + physicalDamageMultiplier + buffBonus);
    }

    public virtual float GetMagicDamage()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.DamageBoost) : 0f;
        return (baseMagicDamage + magicDamageBonus) * (1f + magicDamageMultiplier + buffBonus);
    }

    public virtual float GetMovementSpeed()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.SpeedBoost) : 0f;
        return movementSpeed + (baseSpeed * buffBonus);
    }

    public virtual float GetCritChance()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.CritChance) : 0f;
        // Giới hạn tỉ lệ chí mạng tối đa là 100% (tương đương 1.0)
        return Mathf.Clamp(critChance + buffBonus, 0f, 1f);
    }

    public virtual float GetCritDamage()
    {
        return critDamageMultiplier;
    }

    public virtual float GetDefense()
    {
        return (physicalArmor + magicArmor) / 2f;
    }

    public virtual float GetMaxHealth()
    {
        return maxHealth;
    }

    // ─── Damage Application ────────────────────────────────────────────────────
    public virtual void TakeMixedDamage(float physicalDamage, float magicDamage, float armorPenetration = 0f, float magicPenetration = 0f, bool isCrit = false)
    {
        float finalPhys = 0f;
        float finalMagic = 0f;

        if (physicalDamage > 0)
        {
            float effectiveArmor = physicalArmor * (1f - armorPenetration);
            finalPhys = CalculateMitigatedDamage(physicalDamage, effectiveArmor);
        }

        if (magicDamage > 0)
        {
            float effectiveMagicArmor = magicArmor * (1f - magicPenetration);
            finalMagic = CalculateMitigatedDamage(magicDamage, effectiveMagicArmor);
        }

        float totalFinalDamage = finalPhys + finalMagic;

        if (totalFinalDamage > 0)
        {
            ProcessDamage(totalFinalDamage, isCrit);
            Debug.Log($"[{gameObject.name}] Took Mixed Damage: {totalFinalDamage:F1} (Phys: {finalPhys:F1}, Magic: {finalMagic:F1}). Health: {currentHealth}/{maxHealth}");
        }
    }

    public virtual void TakePhysicalDamage(float damage, float armorPenetration = 0f, bool isCrit = false)
    {
        TakeMixedDamage(damage, 0f, armorPenetration, 0f, isCrit);
    }

    public virtual void TakeMagicDamage(float damage, float resistancePenetration = 0f, bool isCrit = false)
    {
        TakeMixedDamage(0f, damage, 0f, resistancePenetration, isCrit);
    }

    protected float CalculateMitigatedDamage(float rawDamage, float armorValue)
    {
        if (armorValue >= 0)
        {
            return rawDamage * (100f / (100f + armorValue));
        }
        else
        {
            return rawDamage * (2f - (100f / (100f - armorValue)));
        }
    }

    public virtual void TakeDamage(float damage, float armorPenetration = 0f)
    {
        // Backwards compatibility alias for TakePhysicalDamage
        TakePhysicalDamage(damage, armorPenetration);
    }

    protected virtual void ProcessDamage(float finalDamage, bool isCrit = false)
    {
        lastDamageTime = Time.time;

        // Apply shield first
        if (shield > 0)
        {
            float shieldDamage = Mathf.Min(finalDamage, shield);
            shield -= shieldDamage;
            finalDamage -= shieldDamage;
            shieldRechargeTimer = shieldRechargeCooldown;
        }

        if (finalDamage > 0)
        {
            // Spawn exactly at character position, the PopUp script handles its own offset
            Vector3 hitPosition = transform.position; 
            if (damagePopupPrefab != null)
            {
                GameObject damagePopup = Instantiate(damagePopupPrefab, hitPosition, Quaternion.identity);
                damagePopup.GetComponent<PopUpDamage>().Setup(Mathf.RoundToInt(finalDamage), isCrit);
            }
            currentHealth -= finalDamage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"[{gameObject.name}] Healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    public virtual void RestoreShield(float amount)
    {
        shield += amount;
        shield = Mathf.Min(shield, maxShield);
        shieldRechargeTimer = shieldRechargeCooldown;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator == null)
        {
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.SetTrigger("death");
        }
    }
}
