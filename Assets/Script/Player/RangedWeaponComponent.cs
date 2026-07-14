using UnityEngine;

public class RangedWeaponComponent : WeaponComponent
{
    [Header("Projectile Settings")]
    public Transform attackPoint;
    public GameObject projectilePrefabOverride; // Tùy chọn đè lên prefab của WeaponData
    public float projectileSpeed = 20f;
    public float destroyTime = 3f;

    private float physicalDamage;
    private float magicDamage;
    private float physicalDamageBonus;
    private float magicDamageBonus;
    private float critChance;

    public override void Initialize(WeaponController weaponController)
    {
        base.Initialize(weaponController);

        // Đọc chỉ số từ WeaponData
        physicalDamage = 0f;
        magicDamage = 0f;
        physicalDamageBonus = 0f;
        magicDamageBonus = 0f;
        critChance = 0f;

        if (controller.currentItemData != null)
        {
            foreach (var mod in controller.currentItemData.modifiers)
            {
                string statLower = mod.stat.ToLower();
                float flatVal = mod.value;
                float percentVal = mod.percentValue / 100f;
                float logicVal = mod.percent ? (mod.percentValue != 0 ? percentVal : flatVal / 100f) : flatVal;

                if (statLower == "physical damage" || statLower == "physicaldamage") 
                    physicalDamage += logicVal;
                else if (statLower == "magic damage" || statLower == "magicdamage") 
                    magicDamage += logicVal;
                else if (statLower == "physical damage bonus" || statLower == "physicaldamagebonus") 
                    physicalDamageBonus += logicVal;
                else if (statLower == "magic damage bonus" || statLower == "magicdamagebonus") 
                    magicDamageBonus += logicVal;
                else if (statLower == "crit chance" || statLower == "critchance") 
                    critChance += logicVal;
            }
        }

        if (attackPoint == null)
        {
            attackPoint = this.transform; 
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "HitFrame")
        {
            Debug.Log($"arrow position: {attackPoint.position}");
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject prefabToUse = projectilePrefabOverride;
        if (prefabToUse == null && controller.weaponData != null)
        {
            prefabToUse = controller.weaponData.projectilePrefab;
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning("[RangedWeaponComponent] Projectile Prefab is not assigned!");
            return;
        }

        // Hệ thống ngắm theo Camera
        Vector3 targetPoint = attackPoint.position + controller.transform.root.forward * 100f; // Fallback
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            
            // Sắp xếp các điểm chạm theo khoảng cách gần -> xa
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            bool hitFound = false;
            foreach (var hit in hits)
            {
                // Bỏ qua cơ thể nhân vật (để tia không bị kẹt sau lưng) và các vùng trigger
                if (!hit.transform.IsChildOf(controller.transform.root) && !hit.collider.isTrigger)
                {
                    targetPoint = hit.point;
                    hitFound = true;
                    break;
                }
            }
            
            if (!hitFound)
            {
                targetPoint = ray.GetPoint(100f); // Bắn xa tít nếu không chạm gì
            }
        }

        Vector3 shootDirection = (targetPoint - attackPoint.position).normalized;
        Quaternion shootRotation = Quaternion.LookRotation(shootDirection);

        GameObject projectile;
        if (ProjectilePool.Instance != null)
        {
            projectile = ProjectilePool.Instance.SpawnFromPool(prefabToUse, attackPoint.position, shootRotation);
        }
        else
        {
            projectile = Instantiate(prefabToUse, attackPoint.position, shootRotation);
        }
        
        ProjectileBase projScript = projectile.GetComponent<ProjectileBase>();
        if (projScript == null)
        {
            Debug.LogWarning("[RangedWeaponComponent] Projectile Prefab does not have a ProjectileBase script!");
            return;
        }

        // Tính toán sát thương thực tế
        float playerCorePhys = PlayerStat.Instance != null ? PlayerStat.Instance.basePhysicalDamage : 0f;
        float playerGlobalPhysBonus = PlayerStat.Instance != null ? PlayerStat.Instance.physicalDamageMultiplier : 0f;
        float finalPhys = (playerCorePhys + physicalDamage) * (1f + this.physicalDamageBonus + playerGlobalPhysBonus);

        float playerCoreMagic = PlayerStat.Instance != null ? PlayerStat.Instance.baseMagicDamage : 0f;
        float playerGlobalMagicBonus = PlayerStat.Instance != null ? PlayerStat.Instance.magicDamageMultiplier : 0f;
        float finalMagic = (playerCoreMagic + magicDamage) * (1f + this.magicDamageBonus + playerGlobalMagicBonus);

        bool isCrit = false;
        if (PlayerStat.Instance != null)
        {
            float totalCritChance = Mathf.Clamp(PlayerStat.Instance.GetCritChance() + this.critChance, 0f, 1f);
            if (Random.value <= totalCritChance)
            {
                isCrit = true;
                // Áp dụng sát thương chí mạng cho CẢ sát thương vật lý và phép thuật
                float critMultiplier = PlayerStat.Instance.GetCritDamage();
                finalPhys *= critMultiplier;
                finalMagic *= critMultiplier;
            }
        }
        
        DamagePayload payload = new DamagePayload(finalPhys, finalMagic, isCrit, controller.transform.root, controller.currentItemData);

        projScript.Initialize(projectileSpeed, destroyTime, payload);

        Debug.Log($"[RangedWeaponComponent] Fired a projectile from {attackPoint.name}. Phys: {finalPhys:F1}, Magic: {finalMagic:F1}, Crit: {isCrit}");
    }
}
