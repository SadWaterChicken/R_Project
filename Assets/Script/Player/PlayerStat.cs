using UnityEngine;

public class PlayerStat : MonoBehaviour
{
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
    



}
