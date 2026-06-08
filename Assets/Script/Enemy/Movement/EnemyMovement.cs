using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    public Transform target;
    [SerializeField] private Animator animator;
    public float Updaterate = 3f;
    private NavMeshAgent agent;
    private AgentLinkMover linkMover;
    private const string Jump = "Jump";
    private const string Land = "Land";
    private const string Idle = "IsWalking";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        linkMover = GetComponent<AgentLinkMover>();

        linkMover.OnLinkEnd += HandleLinkEnd;
        linkMover.OnLinkStart += HandleLinkStart;
    }

    private void Start()
    {
        StartCoroutine(FollowTarget());
    }

    private void HandleLinkStart()
    {
        animator.SetTrigger(Jump);
    }
    private void HandleLinkEnd()
    {
        animator.SetTrigger(Land);
    }

    private void Update()
    {
        animator.SetBool("IsWalking", agent.velocity.magnitude > 0.01f);
    }
    private IEnumerator FollowTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(Updaterate);
        while (enabled)
        {
            agent.SetDestination(target.transform.position - (target.transform.position - transform.position).normalized * 0.5f);
            yield return wait;
        }
    }
}
