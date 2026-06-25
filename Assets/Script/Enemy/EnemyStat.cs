using UnityEngine;
using System.Collections;

public class EnemyStat : CharacterStats
{
    [Header("General")]
    public int enemyLevel = 1;

    [Header("Combat Ranges")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;

    public float outOfCombatDelay = 3f;

    protected override void Start()
    {
        base.Start();

        // Check if there's a difficulty multiplier applied by the Dungeon Spawner
        RoomEnemyTracker tracker = GetComponent<RoomEnemyTracker>();
        if (tracker != null && tracker.statMultiplier != 1.0f)
        {
            float multi = tracker.statMultiplier;
            
            // Example of Level Scaling: +10% stats per level beyond 1
            float levelScale = 1f + (Mathf.Max(1, enemyLevel) - 1) * 0.1f;
            float finalMulti = multi * levelScale;

            // Apply multiplier to core stats
            maxHealth = Mathf.RoundToInt(maxHealth * finalMulti);
            currentHealth = maxHealth;
            basePhysicalDamage *= finalMulti;
            baseMagicDamage *= finalMulti;
            // (Armor and speed are kept original so enemies don't become too fast or unkillable, 
            // but you can scale them here too if you want)
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // Regenerate health if out of combat
        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            if (Time.time - lastDamageTime >= outOfCombatDelay)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }
    }

    public void changeHealth(float damage)
    {
        // Legacy method, applies raw damage (true damage) bypassing armor
        ProcessDamage(damage);
    }

    protected override void Die()
    {
        if (isDead) return;
        base.Die();
        
        // Đóng băng vật lý để xác không bị rớt xuyên qua mặt đất
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Vô hiệu hóa hitbox/collider để không cản đường
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Dừng NavMeshAgent để quái vật đứng yên tại chỗ
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Tắt toàn bộ các Script khác (Combat, Movement...) để quái vật ngừng hoạt động
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this) script.enabled = false;
        }
        
        // Tắt thanh máu (Canvas UI)
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null) canvas.gameObject.SetActive(false);
        
        // Chờ 1 giây cho animation bốc hơi chạy xong rồi mới xóa
        Destroy(gameObject, 1f);
    }
}
