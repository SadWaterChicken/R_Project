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
    public float basePhysicalDamage ;
    public float physicalDamageBonus ;
    public float baseMagicDamage ;
    public float magicDamageBonus ;

    [Header("Defence Stats")]
    public float physicalArmor ;
    public float magicArmor ;

    [Header("Movement & Combat")]
    public float baseSpeed ;
    public float movementSpeed ;
    public float attackSpeed ;

    [Header("Luck & Crit")]
    public float critChance ;

    [Header("Shield Stats")]
    public float shield;
    public float maxShield;
    public float shieldRegenRate;
    public float shieldRechargeCooldown;
    
    protected float shieldRechargeTimer;
    protected float lastDamageTime;

    protected CharacterBuffManager buffManager;

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
        return basePhysicalDamage + physicalDamageBonus + (basePhysicalDamage * buffBonus);
    }

    public virtual float GetMagicDamage()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.DamageBoost) : 0f;
        return baseMagicDamage + magicDamageBonus + (baseMagicDamage * buffBonus);
    }

    public virtual float GetMovementSpeed()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.SpeedBoost) : 0f;
        return movementSpeed + (baseSpeed * buffBonus);
    }

    public virtual float GetCritChance()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.CritChance) : 0f;
        return Mathf.Clamp01(critChance + buffBonus);
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
    public virtual void TakePhysicalDamage(float damage, float armorPenetration = 0f)
    {
        float effectiveArmor = physicalArmor * (1f - armorPenetration);
        float finalDamage = CalculateMitigatedDamage(damage, effectiveArmor);

        ProcessDamage(finalDamage);
        Debug.Log($"[{gameObject.name}] Took {finalDamage} physical damage. Health: {currentHealth}/{maxHealth}");
    }

    public virtual void TakeMagicDamage(float damage, float resistancePenetration = 0f)
    {
        float effectiveArmor = magicArmor * (1f - resistancePenetration);
        float finalDamage = CalculateMitigatedDamage(damage, effectiveArmor);

        ProcessDamage(finalDamage);
        Debug.Log($"[{gameObject.name}] Took {finalDamage} magic damage. Health: {currentHealth}/{maxHealth}");
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

    protected virtual void ProcessDamage(float finalDamage)
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
        // Spawn the damage popup right before reducing health
        if (damagePopupPrefab != null)
        {
            // Spawn exactly at character position, the PopUp script handles its own offset
            Vector3 hitPosition = transform.position; 
            GameObject damagePopup = Instantiate(damagePopupPrefab, hitPosition, Quaternion.identity);
            damagePopup.GetComponent<PopUpDamage>().Setup(Mathf.RoundToInt(finalDamage));
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

    protected abstract void Die();
}
