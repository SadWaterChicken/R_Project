using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour, IDamageable
{
    [SerializeField] AttackRadius attackRadius;
    [SerializeField] Animator anim;
    private Coroutine lookCoroutine;

    private int health = 300;

    private const string ATTACK_TRIGGER = "Attack";
    private void Awake()
    {
        attackRadius.onAttack += OnAttack;
    }

    private void OnAttack( IDamageable target)
    {
        anim.SetTrigger(ATTACK_TRIGGER);

        if(lookCoroutine != null)
        {
            StopCoroutine(lookCoroutine);
        }
        lookCoroutine = StartCoroutine(LookAt(target.GetTransform()));
    }

    private IEnumerator LookAt(Transform target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        float time = 0;

        while(time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime * 2;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

    public void takeDamage(float Damage)
    {
        health -= (int)Damage;
        if(health < 0)
        {
            gameObject.SetActive(false);
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
