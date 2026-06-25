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

    [Header("Main Hand Skill")]
    [SerializeField] private string mainHandAnimationTrigger = "hit1";
    [SerializeField] private float mainHandCooldown = 4f;
    [SerializeField] private float mainHandReleaseDelay = 0.3f;

    [Header("Off Hand Skill")]
    [SerializeField] private string offHandAnimationTrigger = "hit2";
    [SerializeField] private float offHandCooldown = 4f;
    [SerializeField] private float offHandReleaseDelay = 0.3f;

    [Header("Animation Safety")]
    [SerializeField] private float suppressMeleeEventDuration = 0.85f;

    private float nextMainHandCastTime;
    private float nextOffHandCastTime;
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
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        if (equipmentManager == null)
        {
            equipmentManager = GetComponent<EquipmentManager>();
        }

        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }

        playerAnimationEventHandler = playerAnimator != null
            ? playerAnimator.GetComponent<AnimationEventHandler>()
            : null;

        CacheEquipmentAnimationListener();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && OwnsMainHandSkillInput())
        {
            DetachPlayerCombatEquipmentManagerForThisFrame();
            TryCastMainHandSkill();
        }

        if (Input.GetKeyDown(KeyCode.Q) && OwnsOffHandSkillInput())
        {
            DetachPlayerCombatEquipmentManagerForThisFrame();
            TryCastOffHandSkill();
        }
    }

    private void LateUpdate()
    {
        RestorePlayerCombatEquipmentManager();
    }

    public bool TryCastMainHandSkill()
    {
        WeaponController weapon = GetEquippedWeapon("currentMainHandWeapon");
        if (!CanCastSkill(weapon, nextMainHandCastTime))
        {
            return false;
        }

        nextMainHandCastTime = Time.time + GetSkillCooldown(weapon, mainHandCooldown);
        StartCoroutine(CastSkillRoutine(mainHandAnimationTrigger, mainHandReleaseDelay, equipmentManager.TriggerMainHandSkill));
        return true;
    }

    public bool TryCastOffHandSkill()
    {
        WeaponController weapon = GetEquippedWeapon("currentOffHandWeapon");
        if (!CanCastSkill(weapon, nextOffHandCastTime))
        {
            return false;
        }

        nextOffHandCastTime = Time.time + GetSkillCooldown(weapon, offHandCooldown);
        StartCoroutine(CastSkillRoutine(offHandAnimationTrigger, offHandReleaseDelay, equipmentManager.TriggerOffHandSkill));
        return true;
    }

    private IEnumerator CastSkillRoutine(string animationTrigger, float releaseDelay, UnityAction releaseSkill)
    {
        SuppressPlayerMeleeAnimationEvents();
        PlaySkillAnimation(animationTrigger);

        yield return new WaitForSeconds(releaseDelay);
        releaseSkill?.Invoke();
    }

    private void PlaySkillAnimation(string animationTrigger)
    {
        if (playerAnimator == null || string.IsNullOrEmpty(animationTrigger)) return;

        playerAnimator.ResetTrigger("hit1");
        playerAnimator.ResetTrigger("hit2");
        playerAnimator.SetTrigger(animationTrigger);
    }

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

    private bool CanCastSkill(WeaponController weapon, float nextCastTime)
    {
        if (equipmentManager == null || weapon == null || Time.time < nextCastTime)
        {
            return false;
        }

        return weapon.currentItemData != null && weapon.currentItemData.hasSkill;
    }

    private float GetSkillCooldown(WeaponController weapon, float fallbackCooldown)
    {
        if (weapon != null && weapon.currentItemData != null && weapon.currentItemData.hasSkill)
        {
            return Mathf.Max(0f, weapon.currentItemData.weaponSkill.cooldown);
        }

        return fallbackCooldown;
    }

    private WeaponController GetEquippedWeapon(string fieldName)
    {
        if (equipmentManager == null) return null;

        FieldInfo field = typeof(EquipmentManager).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        return field != null ? field.GetValue(equipmentManager) as WeaponController : null;
    }

    private bool OwnsMainHandSkillInput()
    {
        WeaponController weapon = GetEquippedWeapon("currentMainHandWeapon");
        return weapon != null && weapon.currentItemData != null && weapon.currentItemData.hasSkill;
    }

    private bool OwnsOffHandSkillInput()
    {
        WeaponController weapon = GetEquippedWeapon("currentOffHandWeapon");
        return weapon != null && weapon.currentItemData != null && weapon.currentItemData.hasSkill;
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
