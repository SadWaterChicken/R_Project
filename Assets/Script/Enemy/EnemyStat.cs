using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

public class EnemyStat : MonoBehaviour
{
    
    public int maxHealth;
    public float currentHealth;
    public int healAmount = 100;
    private Coroutine healCoroutine;
    private float lastDamageTime;
    public float outOfCombatDelay = 3f; // Delay before healing starts

    private void Start()
    {
        currentHealth = maxHealth;
        lastDamageTime = Time.time;
    }



    void Update()
    {
        // Only heal if out of combat for 3 seconds
        if (currentHealth < maxHealth && Time.time - lastDamageTime >= outOfCombatDelay)
        {
            if (healCoroutine == null)  // Only start if not already running
            {
                healCoroutine = StartCoroutine(HealPerSecond());
            }
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void changeHealth(float damage)
    {
        // Update last damage time
        lastDamageTime = Time.time;
        
        // Stop healing when attacked
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }
        
        currentHealth -= damage;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        } else if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator HealPerSecond()
    {
        while (currentHealth < maxHealth)
        {
            yield return new WaitForSeconds(1f);
            if (currentHealth < maxHealth)
            {
                currentHealth += healAmount;
            }
        }
        healCoroutine = null;
    }

}
