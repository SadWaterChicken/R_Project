using System.Collections;
using UnityEngine;

/// <summary>
/// Player Combat System
/// - Left Click: Normal Attack (chạy animation)
/// - Q: Special Attack (costs mana, chạy animation)
/// - E: Heal using Sanity (chỉ heal mới trừ sanity)
/// </summary>
[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 60f; // cone angle for normal attack (degrees)
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackHitDelay = 0.25f; // fallback delay if no animation event

    [Header("Facing")]
    [SerializeField] private SpriteRenderer spriteRendererForFacing; // optional: read flipX to determine facing
    [SerializeField] private Vector3 leftAttackLocalOffset = new Vector3(-0.25f, 0f, 0f); // shift applied when facing left
    [SerializeField] private Vector3 rightAttackLocalOffset = Vector3.zero; // optional right-side fine tune

    [Header("Special Attack")]
    [SerializeField] private int specialAttackManaCost = 15;
    [SerializeField] private float specialAttackMultiplier = 2f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject normalAttackVFX;
    [SerializeField] private GameObject specialAttackVFX;
    [SerializeField] private Color attackConeColor = new Color(1f, 0.85f, 0.2f, 0.35f);
    [SerializeField] private float attackConeDuration = 0.28f;

    [Header("Audio")]
    [SerializeField] private AudioClip normalAttackSound;
    [SerializeField] private AudioClip specialAttackSound;
    [SerializeField] private AudioClip healSound;

    private PlayerData playerData;
    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb2d;
    private float lastAttackTime;
    private bool canAttack = true;
    private Vector3 attackPointBaseLocalPos;
    [SerializeField] private Vector2 fallbackAttackOriginOffset = new Vector2(1f, 0f);
    private int lastFacingSign = 1;

    // internal state for hit timing
    private bool attackHitApplied = false;
    private int pendingAttackDamage = 0;
    private bool pendingIsSpecial = false;

    // Dash
    [Header("Dash")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space; // use Space for dash
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = -99f;

    // Animation parameter names
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int SpecialAttackTrigger = Animator.StringToHash("SpecialAttack");

    #region Unity Lifecycle

    private void Awake()
    {
        playerData = GetComponent<PlayerData>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb2d = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (attackPoint == null)
        {
            // Tạo attack point nếu không có
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            attackPoint = attackPointObj.transform;
        }
        // store base local position for mirroring
        if (attackPoint != null)
        {
            attackPointBaseLocalPos = attackPoint.localPosition;
            lastFacingSign = GetFacingSign();
        }
        // Auto-assign SpriteRenderer if not set
        if (spriteRendererForFacing == null)
        {
            spriteRendererForFacing = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        UpdateFacingIfNeeded();
        HandleCombatInput();
        UpdateCooldown();
    }

    private void UpdateFacingIfNeeded()
    {
        if (attackPoint == null) return;
        int sign = GetFacingSign();
        if (sign == lastFacingSign) return;
        // Do not modify attackPoint.localPosition here; we'll compute world origin from base offset when applying hits
        lastFacingSign = sign;
    }

    #endregion

    private Vector2 GetFacingVector()
    {
        int sign = GetFacingSign();

        Vector2 baseForward;
        if (attackPoint != null)
        {
            baseForward = attackPoint.right;
        }
        else
        {
            baseForward = transform.right;
        }

        // If baseForward's horizontal sign doesn't match desired sign, flip it
        float apSign = Mathf.Sign(baseForward.x);
        if (apSign == 0) apSign = 1f;
        if ((int)apSign != sign)
        {
            baseForward = -baseForward;
        }

        return baseForward.normalized;
    }

    private int GetFacingSign()
    {
        // Prefer transform scale-based flipping (common for many projects):
        // if localScale.x is negative, the character is facing left.
        if (transform != null && transform.localScale.x < 0f)
        {
            return -1;
        }

        // Fallback to sprite renderer flipX if available
        if (spriteRendererForFacing != null)
        {
            return spriteRendererForFacing.flipX ? -1 : 1;
        }

        // Default to facing right
        return 1;
    }

    #region Input Handling

    private void HandleCombatInput()
    {
        // Left Click - Normal Attack (chuột trái)
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            PerformNormalAttack();
        }

        // Q - Special Attack
        if (Input.GetKeyDown(KeyCode.Q) && canAttack)
        {
            PerformSpecialAttack();
        }

        // E - Heal using Sanity
        if (Input.GetKeyDown(KeyCode.E))
        {
            HealUsingSanity();
        }

        // Dash (Space) with direction from WASD/Arrows (or fallback to facing direction)
        if (Input.GetKeyDown(dashKey))
        {
            if (isDashing)
            {
                Debug.LogWarning("PlayerCombat: Dash input received but isDashing=true. Possible stuck state.");
            }
            else if (Time.time < lastDashTime + dashCooldown)
            {
                Debug.Log($"PlayerCombat: Dash on cooldown. Time remaining: {(lastDashTime + dashCooldown - Time.time):F2}s");
            }
            else
            {
                Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (inputDir.sqrMagnitude < 0.01f)
                {
                    // fallback to facing direction
                    inputDir = transform.right;
                }
                Debug.Log("PlayerCombat: Dash triggered!");
                StartCoroutine(PerformDash(inputDir.normalized));
            }
        }
        
        // Safety: reset isDashing if it's been too long (stuck state recovery)
        if (isDashing && Time.time > lastDashTime + dashDuration + 1f)
        {
            Debug.LogWarning("PlayerCombat: isDashing was stuck, resetting!");
            isDashing = false;
        }
    }

    #endregion

    #region Combat Actions

    private void PerformNormalAttack()
    {
        if (!canAttack) return;

        Debug.Log("Player: Normal Attack!");

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

        // Calculate damage
        int damage = playerData.AttackPower;
        
        // Check for critical hit
        if (Random.value < playerData.CriticalChance)
        {
            damage = Mathf.RoundToInt(damage * playerData.CriticalDamage);
            Debug.Log($"CRITICAL HIT! Damage: {damage}");
        }

        // Store pending attack so animation event can apply it at the correct frame
        attackHitApplied = false;
        pendingAttackDamage = damage;
        pendingIsSpecial = false;

        // Fallback: if no animation event, apply hit after attackHitDelay
        StartCoroutine(HitAfterDelay(attackHitDelay));

    // Visual & Audio feedback (play VFX/sound at start) - spawn at computed origin so it follows facing
    Vector3 startVfxPos = ComputeAttackOriginWorld();
    PlayAttackVFXAt(startVfxPos, normalAttackVFX);
        PlaySound(normalAttackSound);

        // Start cooldown
        StartCooldown();
    }

    private void PerformSpecialAttack()
    {
        if (!canAttack) return;

        // Check mana
        if (!playerData.UseMana(specialAttackManaCost))
        {
            Debug.Log("Not enough mana for special attack!");
            return;
        }

        Debug.Log("Player: Special Attack!");

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(SpecialAttackTrigger);
        }

        // Calculate damage
        int damage = Mathf.RoundToInt(playerData.AttackPower * specialAttackMultiplier);

        // Store pending special attack and fallback hit timing
        attackHitApplied = false;
        pendingAttackDamage = damage;
        pendingIsSpecial = true;

        // Use slightly longer delay for special if needed
        StartCoroutine(HitAfterDelay(attackHitDelay + 0.1f));

    // Visual & Audio feedback - spawn at computed origin so it follows facing
    Vector3 startVfxPos = ComputeAttackOriginWorld();
    PlayAttackVFXAt(startVfxPos, specialAttackVFX);
        PlaySound(specialAttackSound);

        // Start cooldown
        StartCooldown();
    }

    private void HealUsingSanity()
    {
        if (playerData.HealUsingSanity())
        {
            PlaySound(healSound);
            Debug.Log("Healed using Sanity!");
        }
    }

    private IEnumerator HitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!attackHitApplied)
        {
            ApplyPendingAttack();
        }
    }

    /// <summary>
    /// Called by Animation Event at the attack's hit frame. Animator should call this on the Player GameObject.
    /// </summary>
    public void OnNormalAttackHit()
    {
        if (!attackHitApplied && !pendingIsSpecial)
        {
            ApplyPendingAttack();
        }
    }

    public void OnSpecialAttackHit()
    {
        if (!attackHitApplied && pendingIsSpecial)
        {
            ApplyPendingAttack();
        }
    }

    private void ApplyPendingAttack()
    {
        attackHitApplied = true;
        ApplyAttackHit(pendingAttackDamage, pendingIsSpecial);
    }

    private void ApplyAttackHit(int damage, bool isSpecial)
    {
        // Choose angle and range
        float angle = isSpecial ? attackAngle * 1.5f : attackAngle;
        float range = isSpecial ? attackRange * 1.5f : attackRange;

        // Determine forward direction taking sprite flip or scale into account
        Vector2 forward = GetFacingVector();

        // Compute attack origin (use stored base local pos so flipping mirrors correctly)
        int sign = GetFacingSign();
        Vector3 originLocal = attackPointBaseLocalPos;
        // apply base magnitude with sign
        originLocal.x = Mathf.Abs(attackPointBaseLocalPos.x) * sign;
        // Apply side-specific fine-tune offset
        if (sign < 0)
        {
            originLocal += leftAttackLocalOffset;
        }
        else
        {
            originLocal += rightAttackLocalOffset;
        }
        Vector3 origin = transform.TransformPoint(originLocal);

    // Show attack cone visual for debugging/feedback
    AttackConeVisualizer.ShowCone(origin, forward, angle, range, attackConeDuration, attackConeColor);

        // Get all candidates in circle (from computed origin)
        Collider2D[] candidates = Physics2D.OverlapCircleAll(origin, range, enemyLayer);

        Collider2D playerCol = GetComponent<Collider2D>();

        foreach (var col in candidates)
        {
            bool include = false;

            // Use the collider's closest point to the attack origin for accurate cone/distance tests
            Vector2 closest = col.ClosestPoint(origin);
            Vector2 dir = closest - (Vector2)origin;
            float dist = dir.magnitude;

            if (dist <= 0.001f)
            {
                include = true;
            }
            else
            {
                Vector2 ndir = dir.normalized;
                float ang = Vector2.Angle(forward, ndir);
                if (ang <= angle * 0.5f && dist <= range + 0.01f)
                {
                    include = true;
                }
            }

            // Additional check: if collider is overlapping player's body (touching), include it
            if (!include && playerCol != null && col != null)
            {
                var distInfo = Physics2D.Distance(playerCol, col);
                if (distInfo.isOverlapped || distInfo.distance < 0.05f)
                {
                    include = true;
                }
            }

            if (include)
            {
                IDamageable d = col.GetComponent<IDamageable>();
                if (d != null)
                {
                    d.TakeDamage(damage);
                    Debug.Log($"Applied {damage} to {col.name}");
                }
            }
        }
    }

    /// <summary>
    /// Compute the local-space origin for attacks, taking into account the base attackPoint local position
    /// and side-specific offsets so the origin sits in front of the player on both sides.
    /// </summary>
    private Vector3 ComputeAttackOriginLocal()
    {
        if (attackPoint == null)
        {
            return transform.InverseTransformPoint(transform.position + (transform.right * fallbackAttackOriginOffset.x) + (Vector3)fallbackAttackOriginOffset);
        }

        int sign = GetFacingSign();
        Vector3 originLocal = attackPointBaseLocalPos;
        // ensure we mirror the base X according to facing sign
        originLocal.x = Mathf.Abs(attackPointBaseLocalPos.x) * sign;

        // apply side-specific tweak
        if (sign < 0)
        {
            originLocal += leftAttackLocalOffset;
        }
        else
        {
            originLocal += rightAttackLocalOffset;
        }

        return originLocal;
    }

    private Vector3 ComputeAttackOriginWorld()
    {
        Vector3 local = ComputeAttackOriginLocal();
        return transform.TransformPoint(local);
    }

    private void PlayAttackVFXAt(Vector3 worldPos, GameObject vfxPrefab)
    {
        if (vfxPrefab == null) return;
        GameObject vfx = Instantiate(vfxPrefab, worldPos, Quaternion.identity);
        Destroy(vfx, 1f);
    }

    #endregion

    // Bỏ Sanity System region - Sanity chỉ trừ khi Heal (E key)

    #region Cooldown Management

    private void StartCooldown()
    {
        canAttack = false;
        lastAttackTime = Time.time;
    }

    private void UpdateCooldown()
    {
        if (!canAttack && Time.time >= lastAttackTime + attackCooldown)
        {
            canAttack = true;
        }
    }

    #endregion

    #region Visual & Audio

    private void PlayAttackVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, attackPoint.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator PerformDash()
    {
        // old signature kept for compatibility; call with PerformDash(dir) preferred
        yield return PerformDash(transform.right);
    }

    private IEnumerator PerformDash(Vector2 direction)
    {
        isDashing = true;
        lastDashTime = Time.time;

        // temporarily disable attacking during dash
        bool prevCanAttack = canAttack;
        canAttack = false;

        Vector2 start = transform.position;
        Vector2 target = start + (direction.normalized * dashDistance);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector2 pos = Vector2.Lerp(start, target, t);
            if (rb2d != null)
            {
                rb2d.MovePosition(pos);
            }
            else
            {
                transform.position = pos;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure final position
        if (rb2d != null) rb2d.MovePosition(target); else transform.position = target;

        // restore attack state
        canAttack = prevCanAttack;
        isDashing = false;
        yield break;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        // Draw attack range using computed origin so gizmo matches runtime logic
        Vector3 origin = ComputeAttackOriginWorld();

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRange);

        // Draw special attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, attackRange * 1.5f);
    }

    #endregion
}

/// <summary>
/// Interface cho các đối tượng có thể nhận damage
/// </summary>
public interface IDamageable
{
    void TakeDamage(int damage);
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
}
