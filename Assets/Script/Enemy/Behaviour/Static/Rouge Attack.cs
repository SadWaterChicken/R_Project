using UnityEngine;

public class RougeAttack : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Trigger Attack");
            animator.SetTrigger("Attack");
        }
    }
}
