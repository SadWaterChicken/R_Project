using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-10000)]
public class PlayerSkillCastController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animation Safety")]
    [SerializeField] private float suppressMeleeEventDuration = 0.85f;
    [SerializeField] private float defaultReleaseDelay = 0.3f; // Độ trễ trước khi sinh skill prefab

    private float mainHandNextCastTime = 0f;
    private float offHandNextCastTime = 0f;
    
    private UnityAction<string> equipmentAnimationListener;
    private AnimationEventHandler playerAnimationEventHandler;
    private Coroutine restoreAnimationEventsRoutine;
    private EquipmentManager cachedPlayerCombatEquipmentManager;
    private bool playerCombatEquipmentManagerDetached;

    private void Awake()
    {
        RefreshReferences();
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        if (playerAnimator == null)
        {
            playerAnimator = playerCombat != null && playerCombat.animator != null
                ? playerCombat.animator
                : GetComponentInChildren<Animator>();
        }

        if (equipmentManager == null)
        {
            equipmentManager = playerCombat != null && playerCombat.equipmentManager != null
                ? playerCombat.equipmentManager
                : GetComponent<EquipmentManager>();
        }

        playerAnimationEventHandler = playerAnimator != null
            ? playerAnimator.GetComponent<AnimationEventHandler>()
            : null;

        CacheEquipmentAnimationListener();
    }

    private void Update()
    {
        RefreshReferences();

        if (PlayerSkillManager.Instance == null) return;

        // Phím Q để dùng kỹ năng ở Slot 1
        if (Input.GetKeyDown(KeyCode.Q))
        {
            string skillID = PlayerSkillManager.Instance.equippedSkillSlot1;
            if (!string.IsNullOrEmpty(skillID))
            {
                ActiveSkillData skillData = PlayerSkillManager.Instance.GetSkillByID(skillID);
                if (skillData != null)
                {
                    DetachPlayerCombatEquipmentManagerForThisFrame();
                    TryCastWeaponSkill(skillData, false);
                }
            }
        }

        // Phím E để dùng kỹ năng ở Slot 2
        if (Input.GetKeyDown(KeyCode.E))
        {
            string skillID = PlayerSkillManager.Instance.equippedSkillSlot2;
            if (!string.IsNullOrEmpty(skillID))
            {
                ActiveSkillData skillData = PlayerSkillManager.Instance.GetSkillByID(skillID);
                if (skillData != null)
                {
                    DetachPlayerCombatEquipmentManagerForThisFrame();
                    TryCastWeaponSkill(skillData, true);
                }
            }
        }
    }

    private void LateUpdate()
    {
        RestorePlayerCombatEquipmentManager();
    }

    public bool TryCastWeaponSkill(ActiveSkillData skillData, bool isOffHand)
    {
        if (skillData == null) return false;

        float nextCastTime = isOffHand ? offHandNextCastTime : mainHandNextCastTime;
        if (Time.time < nextCastTime)
        {
            // Đang trong thời gian hồi chiêu
            return false;
        }

        // Kiểm tra vũ khí cầm trên tay có hợp lệ với yêu cầu của skill không
        if (!IsWeaponValidForSkill(skillData, isOffHand))
        {
            Debug.Log($"[PlayerSkillCastController] Vũ khí tay {(isOffHand ? "Phụ" : "Chính")} không tương thích với skill {skillData.skillName}!");
            return false;
        }

        // Kiểm tra Mana (có tính ManaSave modifier cho cả 2 skill)
        float manaCost = skillData.manaCost;
        if (SwordSkillTreeManager.Instance != null)
        {
            var mods = SwordSkillTreeManager.Instance.GetCurrentModifiers();

            bool isWind = skillData.skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                          skillData.name.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool isFire = skillData.skillID.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                          skillData.name.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (mods.windManaSave && isWind) manaCost *= 0.7f;
            if (mods.fireManaSave && isFire) manaCost *= 0.7f;
        }
        if (PlayerStat.Instance != null && !PlayerStat.Instance.ConsumeMana(manaCost))
        {
            Debug.Log($"[PlayerSkillCastController] Không đủ Mana để thi triển {skillData.skillName}!");
            return false;
        }

        // Tính cooldown thực tế (có tính CooldownDown modifier cho WindSlash)
        float realCooldown = skillData.cooldown;
        if (SwordSkillTreeManager.Instance != null)
        {
            var mods = SwordSkillTreeManager.Instance.GetCurrentModifiers();
            bool isWind = skillData.skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                          skillData.name.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (mods.windCooldownDown && isWind) realCooldown = Mathf.Min(realCooldown, 3f);
        }

        // Bắt đầu hồi chiẻu cho Tay tương ứng
        if (isOffHand)
            offHandNextCastTime = Time.time + realCooldown;
        else
            mainHandNextCastTime = Time.time + realCooldown;

        StartCoroutine(CastSkillRoutine(skillData, isOffHand));
        return true;
    }

    private IEnumerator CastSkillRoutine(ActiveSkillData skillData, bool isOffHand)
    {
        SuppressPlayerMeleeAnimationEvents();
        PlaySkillAnimation(skillData.animationTrigger);

        yield return new WaitForSeconds(defaultReleaseDelay);
        SpawnSkillDirectly(skillData, isOffHand);
    }

    private void PlaySkillAnimation(string animationTrigger)
    {
        if (playerAnimator == null || string.IsNullOrEmpty(animationTrigger)) return;

        // Reset các trigger cũ (nếu có, ví dụ hit1, hit2) để tránh kẹt
        playerAnimator.ResetTrigger("hit1");
        playerAnimator.ResetTrigger("hit2");
        
        playerAnimator.SetTrigger(animationTrigger);
    }

    private void SpawnSkillDirectly(ActiveSkillData skillData, bool isOffHand)
    {
        if (skillData.skillPrefab == null)
        {
            Debug.LogWarning($"[PlayerSkillCastController] Skill {skillData.skillName} thiếu prefab.");
            return;
        }

        Vector3 spawnPosition = GetSkillSpawnPosition();
        Quaternion spawnRotation = Quaternion.LookRotation(GetPlayerFacingDirection());
        GameObject skillObject = Instantiate(skillData.skillPrefab, spawnPosition, spawnRotation);

        BaseSkill skill = skillObject.GetComponent<BaseSkill>();
        if (skill == null) return;

        float weaponPhysDmg = GetWeaponPhysicalDamage(isOffHand);
        skill.Initialize(PlayerStat.Instance, skillData.baseDamageMultiplier, weaponPhysDmg);

        if (SwordSkillTreeManager.Instance != null)
        {
            SwordSkillModifiers mods = SwordSkillTreeManager.Instance.GetCurrentModifiers();

            // Áp modifier WindSlash
            if (skill is WindSlashSkill windSlash)
                SwordSkillTreeManager.Instance.ApplyWindSlashMods(windSlash, mods);

            // Áp modifier FireBladeSlash
            if (skill is FireBladeSlashSkill fireSkill)
                SwordSkillTreeManager.Instance.ApplyFireBladeMods(fireSkill, mods);
        }

        skill.ExecuteSkill();
    }

    private bool IsWeaponValidForSkill(ActiveSkillData skillData, bool isOffHand)
    {
        if (string.IsNullOrEmpty(skillData.weaponClassRequirement)) return true;

        string fieldName = isOffHand ? "currentOffHandWeapon" : "currentMainHandWeapon";
        WeaponController weapon = GetEquippedWeapon(fieldName);
        if (weapon != null && weapon.currentItemData != null)
        {
            if (weapon.currentItemData.weaponClassName.ToLower() == skillData.weaponClassRequirement.ToLower())
                return true;
        }

        return false;
    }

    private float GetWeaponPhysicalDamage(bool isOffHand)
    {
        string fieldName = isOffHand ? "currentOffHandWeapon" : "currentMainHandWeapon";
        WeaponController weapon = GetEquippedWeapon(fieldName);
        if (weapon == null || weapon.currentItemData == null || weapon.currentItemData.modifiers == null)
            return 0f;

        float dmg = 0f;
        foreach (ItemData.StatMod mod in weapon.currentItemData.modifiers)
        {
            string stat = mod.stat.ToLower();
            if (stat == "physical damage" || stat == "physicaldamage" || stat == "physicaldamagebonus")
            {
                dmg += mod.value;
            }
        }
        return dmg;
    }

    // ─── UTILS (Kế thừa từ bản cũ) ───────────────────────────────────────────

    private void SuppressPlayerMeleeAnimationEvents()
    {
        RefreshReferences();
        if (playerAnimationEventHandler == null || equipmentAnimationListener == null) return;

        playerAnimationEventHandler.OnEventTriggered?.RemoveListener(equipmentAnimationListener);

        if (restoreAnimationEventsRoutine != null)
        {
            StopCoroutine(restoreAnimationEventsRoutine);
        }

        restoreAnimationEventsRoutine = StartCoroutine(RestorePlayerMeleeAnimationEventsAfterDelay());
    }

    private IEnumerator RestorePlayerMeleeAnimationEventsAfterDelay()
    {
        yield return new WaitForSeconds(suppressMeleeEventDuration);

        if (playerAnimationEventHandler != null && equipmentAnimationListener != null)
        {
            playerAnimationEventHandler.OnEventTriggered.RemoveListener(equipmentAnimationListener);
            playerAnimationEventHandler.OnEventTriggered.AddListener(equipmentAnimationListener);
        }

        restoreAnimationEventsRoutine = null;
    }

    private void CacheEquipmentAnimationListener()
    {
        if (equipmentManager == null) return;

        MethodInfo method = typeof(EquipmentManager).GetMethod(
            "ForwardEventToWeapons",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (method == null) return;

        equipmentAnimationListener = (UnityAction<string>)System.Delegate.CreateDelegate(
            typeof(UnityAction<string>),
            equipmentManager,
            method);
    }

    private WeaponController GetEquippedWeapon(string fieldName)
    {
        if (equipmentManager == null) return null;

        FieldInfo field = typeof(EquipmentManager).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        return field != null ? field.GetValue(equipmentManager) as WeaponController : null;
    }

    private Vector3 GetSkillSpawnPosition()
    {
        PlayerStat player = PlayerStat.Instance;
        if (player == null) return transform.position;

        Vector3 direction = GetPlayerFacingDirection();
        return player.transform.position + Vector3.up + direction * 0.8f;
    }

    private Vector3 GetPlayerFacingDirection()
    {
        Transform activeCamera = Camera.main != null ? Camera.main.transform : null;
        if (activeCamera != null)
        {
            Vector3 direction = activeCamera.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                return direction.normalized;
            }
        }

        PlayerStat player = PlayerStat.Instance;
        if (player != null)
        {
            Vector3 fallback = player.transform.forward;
            fallback.y = 0f;
            if (fallback.sqrMagnitude > 0.001f)
            {
                return fallback.normalized;
            }
        }

        return Vector3.forward;
    }

    private void DetachPlayerCombatEquipmentManagerForThisFrame()
    {
        if (playerCombat == null || playerCombatEquipmentManagerDetached) return;

        cachedPlayerCombatEquipmentManager = playerCombat.equipmentManager;
        playerCombat.equipmentManager = null;
        playerCombatEquipmentManagerDetached = true;
    }

    private void RestorePlayerCombatEquipmentManager()
    {
        if (playerCombat == null || !playerCombatEquipmentManagerDetached) return;

        playerCombat.equipmentManager = cachedPlayerCombatEquipmentManager;
        cachedPlayerCombatEquipmentManager = null;
        playerCombatEquipmentManagerDetached = false;
    }
}
