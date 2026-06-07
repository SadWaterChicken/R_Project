using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRadius : MonoBehaviour
{

    private List<IDamageable> damageables = new List<IDamageable>();
    public int damaege = 10;
    public float attackCooldown = 1f;

    public delegate void AttackEvent(IDamageable target);
    public AttackEvent onAttack;
    private Coroutine attackCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !damageables.Contains(damageable))
        {
            damageables.Add(damageable);
            if (attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(Attack());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && damageables.Contains(damageable))
        {
            damageables.Remove(damageable);
            if (damageables.Count == 0 && attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }
    }

    private IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(attackCooldown);


        yield return wait; // Initial delay before the first attack

        IDamageable closestDamageable = null;
        float closestDistaince = float.MaxValue;

        while (damageables.Count > 0) {
            for (int i = 0; i < damageables.Count; i++) {
                Transform damageableTransform = damageables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damageableTransform.position);

                if (distance < closestDistaince) {
                    closestDistaince = distance;
                    closestDamageable = damageables[i];
                }
            }
            if (closestDamageable != null)
            {
                onAttack?.Invoke(closestDamageable);
                closestDamageable.takeDamage(damaege);
            }

            closestDamageable = null;
            closestDistaince = float.MaxValue;

            yield return wait;

            damageables.RemoveAll(DisableDamageable);
        }

        attackCoroutine = null;
    }

    private bool DisableDamageable(IDamageable damageable)
    {
        return damageable != null && damageable.GetTransform().gameObject.activeSelf;
    }
}
