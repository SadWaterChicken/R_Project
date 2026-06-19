using UnityEngine;

public class DamageRecieve : MonoBehaviour
{
    public EnemyStat enemy;   // Drag your enemy GameObject here in the Inspector
    public float damageAmount = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyDamage();
    }

    private void ApplyDamage()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            if(enemy != null)
            {
                enemy.changeHealth(damageAmount);
                Debug.Log("Enemy damaged for " + damageAmount);
            }
        }
    }
}
