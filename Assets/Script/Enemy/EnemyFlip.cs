using UnityEngine;
using UnityEngine.AI;

public class EnemyFlip : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Transform attackPoint;
    public SimpleDetector detector; // Link to your detector to get the player
    public NavMeshAgent agent;      // Optional: If you want to flip based on movement instead

    [Header("Settings")]
    public bool flipBasedOnMovement = false; 

    private Vector3 originalAttackPointPos;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (detector == null) detector = GetComponent<SimpleDetector>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (attackPoint != null)
        {
            originalAttackPointPos = attackPoint.localPosition;
        }
    }

    void Update()
    {
        bool shouldFlip = spriteRenderer.flipX;
        bool hasMovedOrTurned = false;

        // Cách 1: Flip dựa trên hướng di chuyển của NavMeshAgent
        if (flipBasedOnMovement && agent != null)
        {
            if (Mathf.Abs(agent.velocity.x) > 0.1f)
            {
                shouldFlip = agent.velocity.x < 0;
                hasMovedOrTurned = true;
            }
        }
        // Cách 2: Flip dựa trên vị trí của Player (Luôn nhìn về phía Player)
        else if (!flipBasedOnMovement && detector != null && detector.player != null)
        {
            float directionX = detector.player.position.x - transform.position.x;
            if (Mathf.Abs(directionX) > 0.1f)
            {
                shouldFlip = directionX < 0;
                hasMovedOrTurned = true;
            }
        }

        // Áp dụng lật Sprite và AttackPoint nếu có sự thay đổi
        if (hasMovedOrTurned)
        {
            spriteRenderer.flipX = shouldFlip;

            if (attackPoint != null)
            {
                // Lật vị trí x của attackPoint giống như bên PlayerController
                attackPoint.localPosition = new Vector3(
                    shouldFlip ? -originalAttackPointPos.x : originalAttackPointPos.x, 
                    originalAttackPointPos.y, 
                    originalAttackPointPos.z
                );
            }
        }
    }
}
