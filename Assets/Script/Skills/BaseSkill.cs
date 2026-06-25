using UnityEngine;

public abstract class BaseSkill : MonoBehaviour
{
    protected PlayerStat caster;
    protected float damageMultiplier = 1f;
    protected float weaponPhysicalDamage;

    public virtual void Initialize(PlayerStat casterStat, float multiplier, float weaponDamage)
    {
        caster = casterStat;
        damageMultiplier = Mathf.Max(0f, multiplier);
        weaponPhysicalDamage = Mathf.Max(0f, weaponDamage);
    }

    public abstract void ExecuteSkill();

    protected float CalculatePhysicalDamage()
    {
        float playerDamage = caster != null ? caster.GetPhysicalDamage() : 0f;
        return (playerDamage + weaponPhysicalDamage) * damageMultiplier;
    }
}
