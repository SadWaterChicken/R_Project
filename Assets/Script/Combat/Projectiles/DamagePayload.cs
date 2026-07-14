using UnityEngine;

[System.Serializable]
public struct DamagePayload
{
    public float physicalDamage;
    public float magicDamage;
    public bool isCrit;
    public Transform owner;
    public ItemData weaponSource;

    public DamagePayload(float physDmg, float magicDmg, bool crit, Transform sourceOwner, ItemData sourceWeapon = null)
    {
        physicalDamage = physDmg;
        magicDamage = magicDmg;
        isCrit = crit;
        owner = sourceOwner;
        weaponSource = sourceWeapon;
    }
}
