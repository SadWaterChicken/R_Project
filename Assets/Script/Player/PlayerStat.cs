using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }

    public int maxHealth;
    public float currentHealth;
    public float healthRegenRate;
    public float physicalDamage;
    public float magicDamage;
    public float defense;
    public float sanity;
    public float sanityRegenRate;
    public float mana;
    public float manaRegenRate;
    public int gold = 0;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle player death (e.g., play animation, disable controls, etc.)
        Debug.Log("Player has died.");
    }

    public int GetGold()
    {
        return gold;
    }

    public void AddGold(int amount)
    {
        if (amount > 0)
            gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }
}
