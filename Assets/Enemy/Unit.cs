using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour 
{
    public Transform target;

    [Header("Movement")]
    public float speed = 20f;
    public float turnDst = 5f;
    public float turnSpeed = 3f;
    public float pathUpdateInterval = 0.25f;

    [Header("State / Ranges")]
    public float detectRadius = 6f;
    public float chaseRadius = 16f;
    public float homeArriveEpsilon = 0.05f;
    public float attackRadius = 0f;

    private enum AIState { Idle, Chase, Return, Attack }
    private AIState currentState = AIState.Idle;

    private Path path;
    private PathRequestManager pathRequestManager;
    private Grid grid;
    private Coroutine followCoroutine;
    private Coroutine updatePathCoroutine;

    private Vector3 homeTilePos;
    private bool homeSet = false;

    private void Awake()
    {
        pathRequestManager = FindObjectOfType<PathRequestManager>();
        grid = pathRequestManager != null ? pathRequestManager.GetComponent<Grid>() : FindObjectOfType<Grid>();
    }

    private void Start()
    {
        if (pathRequestManager == null || grid == null || target == null) { enabled = false; return; }
        Node n = grid.NodeFromWorldPoint(transform.position);
        if (n != null) { homeTilePos = n.worldPosition; homeSet = true; }

        updatePathCoroutine = StartCoroutine(UpdatePathLoop());
        SetState(AIState.Idle);
    }

    private void OnDisable()
    {
        if (updatePathCoroutine != null) StopCoroutine(updatePathCoroutine);
        if (followCoroutine != null) StopCoroutine(followCoroutine);
        if (grid != null) grid.ClearPathHighlights();
    }

    private void Update()
    {
        float distToTarget = (target != null) ? Vector2.Distance(transform.position, target.position) : float.MaxValue;
        float distToHome = homeSet ? Vector2.Distance(transform.position, homeTilePos) : 0f;

        switch (currentState)
        {
            case AIState.Idle:
                if (target != null && distToTarget <= detectRadius) SetState(AIState.Chase);
                break;
            case AIState.Chase:
                if (target == null) SetState(homeSet ? AIState.Return : AIState.Idle);
                else if (distToTarget > chaseRadius) SetState(homeSet ? AIState.Return : AIState.Idle);
                else if (attackRadius > 0f && distToTarget <= attackRadius) SetState(AIState.Attack);
                break;
            case AIState.Attack:
                if (target == null) SetState(homeSet ? AIState.Return : AIState.Idle);
                else if (attackRadius <= 0f || distToTarget > attackRadius * 1.1f)
                    SetState(distToTarget <= chaseRadius ? AIState.Chase : (homeSet ? AIState.Return : AIState.Idle));
                break;
            case AIState.Return:
                if (target != null && distToTarget <= detectRadius) SetState(AIState.Chase);
                else if (homeSet && distToHome <= homeArriveEpsilon) SetState(AIState.Idle);
                break;
        }
    }

    private void SetState(AIState newState)
    {
        if (currentState == newState) return;

        // entering non-moving: stop follow and clear path highlights
        if (newState != AIState.Chase && newState != AIState.Return)
        {
            if (followCoroutine != null) { StopCoroutine(followCoroutine); followCoroutine = null; }
            path = null;
            if (grid != null) grid.ClearPathHighlights();
        }

        currentState = newState;
    }

    IEnumerator UpdatePathLoop()
    {
        var wait = new WaitForSeconds(pathUpdateInterval);
        while (true)
        {
            if (target != null && grid != null && pathRequestManager != null)
            {
                if (currentState == AIState.Chase || currentState == AIState.Return)
                {
                    Node unitNode = grid.NodeFromWorldPoint(transform.position);
                    Vector3 targetWorld = (currentState == AIState.Return && homeSet) ? homeTilePos : target.position;
                    Node targetNode = grid.NodeFromWorldPoint(targetWorld);

                    if (unitNode != null && targetNode != null && unitNode.walkable && targetNode.walkable)
                    {
                        Vector3 startTilePos = unitNode.worldPosition;
                        Vector3 targetTilePos = targetNode.worldPosition;

                        PathRequestManager.RequestPath(startTilePos, targetTilePos, OnPathFound);
                        Debug.DrawLine(startTilePos, targetTilePos, currentState == AIState.Return ? Color.cyan : Color.yellow, pathUpdateInterval);
                    }
                }
            }
            yield return wait;
        }
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (!pathSuccessful) { if (grid != null) grid.ClearPathHighlights(); return; }
        if (currentState != AIState.Chase && currentState != AIState.Return) return;

        path = new Path(waypoints, transform.position, turnDst);

        // Update tile highlights to the new path
        if (grid != null) grid.SetPathHighlights(waypoints);

        if (followCoroutine != null) StopCoroutine(followCoroutine);
        followCoroutine = StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        if (path == null || path.lookPoints == null || path.lookPoints.Length == 0) yield break;

        int pathIndex = 0;
        Vector3 currentWaypoint = path.lookPoints[pathIndex];

        while (true)
        {
            if (currentState != AIState.Chase && currentState != AIState.Return)
            {
                if (grid != null) grid.ClearPathHighlights();
                yield break;
            }

            Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(currentWaypoint.x, currentWaypoint.y);

            float step = speed * Time.deltaTime;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, step);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            if (Vector2.Distance(newPos, targetPos) < 0.05f)
            {
                pathIndex++;
                if (pathIndex >= path.lookPoints.Length)
                {
                    if (grid != null) grid.ClearPathHighlights();
                    yield break;
                }
                currentWaypoint = path.lookPoints[pathIndex];
            }

            yield return null;
        }
    }
}
