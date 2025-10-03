using System;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] private string playerName = "Hero";

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Mana")]
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana;

    [Header("Sanity")]
    [SerializeField] private int maxSanity = 100;
    [SerializeField] private int currentSanity;
    [SerializeField] private int sanityHealCost = 20; // Sanity cần để heal
    [SerializeField] private int healAmount = 30; // Lượng máu hồi khi dùng sanity
    [SerializeField] private int[] sanityThresholds = new int[] { 75, 50, 25 }; // Các mốc debuff

    [Header("Combat Stats")]
    [SerializeField] private int attackPower = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private int magicPower = 8;
    [SerializeField] private int magicDefense = 5;
    [SerializeField] private float criticalChance = 0.1f; // 10% crit
    [SerializeField] private float criticalDamage = 1.5f; // 150% damage khi crit

    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Currency")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int gems = 0;

    [Header("Save Data")]
    [SerializeField] private string lastSavePointId = "";
    [SerializeField] private Vector3 lastSavePosition;

    // Events
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;
    public event Action<int, int> OnSanityChanged;
    public event Action<int> OnSanityThresholdCrossed; // Trigger debuff khi qua mốc
    public event Action OnPlayerDeath;
    public event Action OnSavePointUsed;

    private int currentSanityDebuffLevel = 0; // 0 = không debuff, 1-3 = level debuff

    #region Properties
    public string PlayerName { get => playerName; set => playerName = value; }
    
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; private set => currentHealth = value; }
    
    public int MaxMana { get => maxMana; set => maxMana = value; }
    public int CurrentMana { get => currentMana; private set => currentMana = value; }
    
    public int MaxSanity { get => maxSanity; set => maxSanity = value; }
    public int CurrentSanity { get => currentSanity; private set => currentSanity = value; }
    public int CurrentSanityDebuffLevel => currentSanityDebuffLevel;
    
    public int AttackPower { get => attackPower; set => attackPower = value; }
    public int Defense { get => defense; set => defense = value; }
    public int MagicPower { get => magicPower; set => magicPower = value; }
    public int MagicDefense { get => magicDefense; set => magicDefense = value; }
    public float CriticalChance { get => criticalChance; set => criticalChance = Mathf.Clamp01(value); }
    public float CriticalDamage { get => criticalDamage; set => criticalDamage = value; }
    
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    
    public int Gold { get => gold; private set => gold = value; }
    public int Gems { get => gems; private set => gems = value; }
    
    public string LastSavePointId { get => lastSavePointId; set => lastSavePointId = value; }
    public Vector3 LastSavePosition { get => lastSavePosition; set => lastSavePosition = value; }
    
    public bool IsAlive => currentHealth > 0;
    #endregion

    private void Awake()
    {
        InitializeStats();
    }

    private void Update()
    {
        if (!IsAlive) return;
    }

    private void InitializeStats()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentSanity = maxSanity;
        currentSanityDebuffLevel = 0;
    }

    #region Health Management
    public void ChangeHealth(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && previousHealth > 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        ChangeHealth(Mathf.Abs(amount));
    }

    public void TakeDamage(int damage)
    {
        // Tính toán damage sau khi trừ defense
        int actualDamage = Mathf.Max(1, damage - defense);
        ChangeHealth(-actualDamage);
    }
    #endregion

    #region Mana Management
    public void ChangeMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            ChangeMana(-amount);
            return true;
        }
        return false;
    }

    public void RestoreMana(int amount)
    {
        ChangeMana(Mathf.Abs(amount));
    }
    #endregion

    #region Sanity Management
    public void ChangeSanity(int amount)
    {
        int previousSanity = currentSanity;
        currentSanity = Mathf.Clamp(currentSanity + amount, 0, maxSanity);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);

        // Check for debuff threshold changes
        CheckSanityDebuff(previousSanity, currentSanity);
    }

    private void CheckSanityDebuff(int previousSanity, int newSanity)
    {
        int previousDebuffLevel = GetDebuffLevel(previousSanity);
        int newDebuffLevel = GetDebuffLevel(newSanity);

        if (previousDebuffLevel != newDebuffLevel)
        {
            currentSanityDebuffLevel = newDebuffLevel;
            OnSanityThresholdCrossed?.Invoke(newDebuffLevel);
            Debug.Log($"Sanity Debuff Level Changed: {newDebuffLevel}");
        }
    }

    private int GetDebuffLevel(int sanityValue)
    {
        // Kiểm tra từ thấp đến cao
        for (int i = sanityThresholds.Length - 1; i >= 0; i--)
        {
            if (sanityValue <= sanityThresholds[i])
            {
                return i + 1; // Level 1, 2, 3
            }
        }
        return 0; // Không có debuff
    }

    /// <summary>
    /// Heal bằng cách đánh đổi Sanity
    /// </summary>
    public bool HealUsingSanity()
    {
        if (currentSanity >= sanityHealCost && currentHealth < maxHealth)
        {
            ChangeSanity(-sanityHealCost);
            Heal(healAmount);
            Debug.Log($"Healed {healAmount} HP by sacrificing {sanityHealCost} Sanity");
            return true;
        }
        else if (currentSanity < sanityHealCost)
        {
            Debug.Log("Not enough Sanity to heal!");
            return false;
        }
        else
        {
            Debug.Log("Already at full health!");
            return false;
        }
    }

    /// <summary>
    /// Restore Sanity về full - chỉ được gọi khi chạm Save Point
    /// </summary>
    public void RestoreSanityAtSavePoint()
    {
        ChangeSanity(maxSanity - currentSanity);
        Debug.Log("Sanity fully restored at Save Point!");
    }
    #endregion

    #region Save Point System
    /// <summary>
    /// Sử dụng Save Point - Hồi đầy HP, Mana, Sanity và lưu vị trí
    /// </summary>
    public void UseSavePoint(string savePointId, Vector3 savePointPosition)
    {
        // Hồi đầy tất cả
        currentHealth = maxHealth;
        currentMana = maxMana;
        RestoreSanityAtSavePoint();

        // Lưu thông tin save point
        lastSavePointId = savePointId;
        lastSavePosition = savePointPosition;

        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnSavePointUsed?.Invoke();

        Debug.Log($"Save Point Used: {savePointId} at {savePointPosition}");
    }

    /// <summary>
    /// Update current position (for auto-save without using SavePoint)
    /// </summary>
    public void UpdateCurrentPosition(string locationId = null)
    {
        // Update position to current transform position
        lastSavePosition = transform.position;
        
        // If no specific location provided, use current position as ID
        if (string.IsNullOrEmpty(locationId))
        {
            locationId = $"AutoSave_{transform.position.x:F0}_{transform.position.y:F0}";
        }
        
        lastSavePointId = locationId;
        
        Debug.Log($"PlayerData: Position updated to {lastSavePosition} at {locationId}");
    }

    /// <summary>
    /// Respawn tại Save Point gần nhất
    /// </summary>
    public void RespawnAtSavePoint()
    {
        if (!string.IsNullOrEmpty(lastSavePointId))
        {
            transform.position = lastSavePosition;
            currentHealth = maxHealth / 2;
            currentMana = maxMana / 2;
            currentSanity = maxSanity / 2;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

            Debug.Log($"Respawned at Save Point: {lastSavePointId}");
        }
    }
    #endregion

    #region Currency
    public void AddGold(int amount)
    {
        gold += Mathf.Abs(amount);
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }
        return false;
    }

    public void AddGems(int amount)
    {
        gems += Mathf.Abs(amount);
    }

    public bool SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            return true;
        }
        return false;
    }
    #endregion

    #region Death
    private void Die()
    {
        Debug.Log("Player has died!");
        OnPlayerDeath?.Invoke();
        // Có thể thêm logic khác như animation, game over screen, etc.
    }

    public void Revive()
    {
        RespawnAtSavePoint();
    }
    #endregion

    #region Save/Load Helpers
    public PlayerDataSnapshot GetSnapshot()
    {
        return new PlayerDataSnapshot
        {
            playerName = this.playerName,
            currentHealth = this.currentHealth,
            maxHealth = this.maxHealth,
            currentMana = this.currentMana,
            maxMana = this.maxMana,
            currentSanity = this.currentSanity,
            maxSanity = this.maxSanity,
            attackPower = this.attackPower,
            defense = this.defense,
            magicPower = this.magicPower,
            magicDefense = this.magicDefense,
            criticalChance = this.criticalChance,
            criticalDamage = this.criticalDamage,
            moveSpeed = this.moveSpeed,
            gold = this.gold,
            gems = this.gems,
            lastSavePointId = this.lastSavePointId,
            lastSavePositionX = this.lastSavePosition.x,
            lastSavePositionY = this.lastSavePosition.y,
            lastSavePositionZ = this.lastSavePosition.z
        };
    }

    public void LoadFromSnapshot(PlayerDataSnapshot snapshot)
    {
        playerName = snapshot.playerName;
        currentHealth = snapshot.currentHealth;
        maxHealth = snapshot.maxHealth;
        currentMana = snapshot.currentMana;
        maxMana = snapshot.maxMana;
        currentSanity = snapshot.currentSanity;
        maxSanity = snapshot.maxSanity;
        attackPower = snapshot.attackPower;
        defense = snapshot.defense;
        magicPower = snapshot.magicPower;
        magicDefense = snapshot.magicDefense;
        criticalChance = snapshot.criticalChance;
        criticalDamage = snapshot.criticalDamage;
        moveSpeed = snapshot.moveSpeed;
        gold = snapshot.gold;
        gems = snapshot.gems;
        lastSavePointId = snapshot.lastSavePointId;
        lastSavePosition = new Vector3(snapshot.lastSavePositionX, snapshot.lastSavePositionY, snapshot.lastSavePositionZ);

        // Update debuff level
        CheckSanityDebuff(maxSanity, currentSanity);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnSanityChanged?.Invoke(currentSanity, maxSanity);
    }

    #region Getter Methods (for debugging and UI)
    public string GetPlayerName() => playerName;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;
    public int GetCurrentSanity() => currentSanity;
    public int GetMaxSanity() => maxSanity;
    public int GetGold() => gold;
    public int GetGems() => gems;
    public string GetLastSavePointId() => lastSavePointId;
    public Vector3 GetLastSavePosition() => lastSavePosition;
    public float GetMoveSpeed() => moveSpeed;
    public int GetAttackPower() => attackPower;
    public int GetDefense() => defense;
    public int GetMagicPower() => magicPower;
    public int GetMagicDefense() => magicDefense;
    public float GetCriticalChance() => criticalChance;
    public float GetCriticalDamage() => criticalDamage;
    #endregion
    #endregion
}

[System.Serializable]
public class PlayerDataSnapshot
{
    public string playerName;
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
    public int maxMana;
    public int currentSanity;
    public int maxSanity;
    public int attackPower;
    public int defense;
    public int magicPower;
    public int magicDefense;
    public float criticalChance;
    public float criticalDamage;
    public float moveSpeed;
    public int gold;
    public int gems;
    public string lastSavePointId;
    public float lastSavePositionX;
    public float lastSavePositionY;
    public float lastSavePositionZ;
}
