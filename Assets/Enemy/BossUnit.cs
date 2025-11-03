using UnityEngine;
using System.Collections;

public class BossUnit : MonoBehaviour
{
    public Transform target;

    [Header("Movement")]
    public float speed = 18f;
    public float turnDst = 5f;
    public float turnSpeed = 3f;
    public float pathUpdateInterval = 0.25f;

    [Header("State / Ranges")]
    public float detectionRadius = 8f;
    public float leashRadius = 16f;
    public float homeArriveEpsilon = 0.05f;

    [Header("Charge Attack")]
    public float chargeCooldown = 3.0f;
    public float chargeWindupTime = 0.15f;
    public float chargeBackstepTiles = 0.3f;
    public float chargeSpeed = 40f;
    public float chargeMaxDuration = 0.45f;

    [Header("Agent Sizes (tiles)")]
    public int agentHalfSizeX = 0;
    public int agentHalfSizeY = 0;
    public int targetHalfSizeX = 0;
    public int targetHalfSizeY = 0;

    private enum AIState { Idle, Chase, Return, Charge }
    private AIState currentState = AIState.Idle;

    private Path path;
    private Coroutine followCoroutine;
    private Coroutine updatePathCoroutine;
    private Coroutine chargeCoroutine;

    private PathRequestManager pathRequestManager;
    private Grid grid;

    private Vector3 homeTilePos;
    private bool homeSet = false;

    private float chargeTimer;

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

        chargeTimer = chargeCooldown;
        updatePathCoroutine = StartCoroutine(UpdatePathLoop());
        SetState(AIState.Idle);
    }

    private void OnDisable()
    {
        if (updatePathCoroutine != null) StopCoroutine(updatePathCoroutine);
        if (followCoroutine != null) StopCoroutine(followCoroutine);
        if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
        if (grid != null) grid.ClearPathHighlights();
    }

    private void Update()
    {
        float distToTarget = (target != null) ? Vector2.Distance(transform.position, target.position) : float.MaxValue;
        float distToHome = homeSet ? Vector2.Distance(transform.position, homeTilePos) : 0f;

        bool inDetect = target != null && distToTarget <= detectionRadius;

        // Charge timer: counts down only while detected; resets when leaving detection
        if (inDetect) { if (chargeTimer > 0f) chargeTimer -= Time.deltaTime; }
        else { chargeTimer = chargeCooldown; }

        // Only decide to start a charge here; once charging, do not interrupt until duration completes.
        Axis align;
        bool chargeReady = (currentState != AIState.Charge) && inDetect && chargeTimer <= 0f && IsAxisAlignedWithPlayer(out align);

        switch (currentState)
        {
            case AIState.Idle:
                if (chargeReady) SetState(AIState.Charge);
                else if (inDetect) SetState(AIState.Chase);
                break;

            case AIState.Chase:
                if (chargeReady) SetState(AIState.Charge);
                else if (!inDetect && distToTarget > leashRadius) SetState(homeSet ? AIState.Return : AIState.Idle);
                break;

            case AIState.Charge:
                // Do nothing here. ChargeRoutine owns the whole charge and exits state only when duration is done.
                break;

            case AIState.Return:
                if (chargeReady) SetState(AIState.Charge);
                else if (inDetect) SetState(AIState.Chase);
                else if (homeSet && distToHome <= homeArriveEpsilon) SetState(AIState.Idle);
                break;
        }
    }

    private enum Axis { None, Horizontal, Vertical }

    private bool IsAxisAlignedWithPlayer(out Axis axis)
    {
        axis = Axis.None;
        if (grid == null || target == null) return false;

        Node bossNode = grid.NodeFromWorldPoint(transform.position);
        Node playerNode = grid.NodeFromWorldPoint(target.position);
        if (bossNode == null || playerNode == null) return false;

        if (bossNode.gridX == playerNode.gridX) { axis = Axis.Vertical; return true; }
        if (bossNode.gridY == playerNode.gridY) { axis = Axis.Horizontal; return true; }
        return false;
    }

    private void SetState(AIState newState)
    {
        if (currentState == newState) return;
        if (currentState == AIState.Charge) StopChargeIfRunning();

        // Stop path following when entering non-moving states
        if (newState != AIState.Chase && newState != AIState.Return)
        {
            if (followCoroutine != null) { StopCoroutine(followCoroutine); followCoroutine = null; }
            path = null;
        }

        currentState = newState;

        if (currentState == AIState.Charge)
        {
            if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
            chargeCoroutine = StartCoroutine(ChargeRoutine());
        }
    }

    private void StopChargeIfRunning()
    {
        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }

    private IEnumerator UpdatePathLoop()
    {
        var wait = new WaitForSeconds(pathUpdateInterval);
        while (true)
        {
            if (grid != null && pathRequestManager != null && target != null)
            {
                if (currentState == AIState.Chase || currentState == AIState.Return)
                {
                    Node unitNode = grid.NodeFromWorldPoint(transform.position);
                    Vector3 targetWorld = (currentState == AIState.Return && homeSet) ? homeTilePos : target.position;
                    Node targetCenter = grid.NodeFromWorldPoint(targetWorld);

                    if (unitNode != null && targetCenter != null)
                    {
                        var rect = new RectInt(
                            targetCenter.gridX - targetHalfSizeX,
                            targetCenter.gridY - targetHalfSizeY,
                            targetHalfSizeX * 2 + 1,
                            targetHalfSizeY * 2 + 1);

                        Node goal = grid.FindClosestClearNodeToRect(rect, unitNode, agentHalfSizeX, agentHalfSizeY);
                        if (goal != null && unitNode.walkable && grid.IsNodeAreaWalkable(unitNode, agentHalfSizeX, agentHalfSizeY))
                        {
                            PathRequestManager.RequestPath(unitNode.worldPosition, goal.worldPosition, OnPathFound);
                            Debug.DrawLine(unitNode.worldPosition, goal.worldPosition, currentState == AIState.Return ? Color.cyan : Color.yellow, pathUpdateInterval);
                        }
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

    private IEnumerator FollowPath()
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

    // Axis-aligned charge (vertical or horizontal)
    private IEnumerator ChargeRoutine()
    {
        // Lock axis and dash target at the start; do not change mid-charge.
        Axis axis;
        if (!IsAxisAlignedWithPlayer(out axis))
        {
            SetState(AIState.Chase);
            yield break;
        }

        Node bossNode = grid.NodeFromWorldPoint(transform.position);
        Node playerNode = grid.NodeFromWorldPoint(target.position);
        if (bossNode == null || playerNode == null)
        {
            SetState(AIState.Chase); yield break;
        }

        Vector3 bossTile = bossNode.worldPosition;
        Vector3 playerTile = playerNode.worldPosition;

        Vector2 dashDir;
        Vector3 dashTarget;
        float tileSize = grid.obstacleTilemap != null ? grid.obstacleTilemap.cellSize.x : 1f;

        if (axis == Axis.Vertical)
        {
            float dy = Mathf.Sign((playerTile.y - bossTile.y) == 0 ? 1 : (playerTile.y - bossTile.y));
            dashDir = new Vector2(0f, dy);
            dashTarget = new Vector3(bossTile.x, playerTile.y, transform.position.z);
        }
        else // Horizontal
        {
            float dx = Mathf.Sign((playerTile.x - bossTile.x) == 0 ? 1 : (playerTile.x - bossTile.x));
            dashDir = new Vector2(dx, 0f);
            dashTarget = new Vector3(playerTile.x, bossTile.y, transform.position.z);
        }

        // Backstep (short)
        Vector3 backstepTarget = bossTile - (Vector3)(dashDir * (chargeBackstepTiles * tileSize));
        Node backNode = grid.NodeFromWorldPoint(backstepTarget);
        if (backNode == null || !backNode.walkable) backstepTarget = bossTile;

        if (chargeWindupTime > 0f) yield return new WaitForSeconds(chargeWindupTime);

        float backstepTime = 0.08f;
        float t = 0f;
        Vector3 startPos = transform.position;
        while (t < backstepTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, backstepTarget, Mathf.Clamp01(t / backstepTime));
            yield return null;
        }
        transform.position = backstepTarget;

        // Charge for the full duration; do not cancel due to detection/leash changes.
        float elapsed = 0f;
        while (elapsed < chargeMaxDuration)
        {
            if (currentState != AIState.Charge) yield break; // external cancel (rare)

            Vector2 cur2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 tgt2D = new Vector2(dashTarget.x, dashTarget.y);

            float step = chargeSpeed * Time.deltaTime;

            // Move until target reached, then hold position for remaining duration
            if (Vector2.Distance(cur2D, tgt2D) > 0.02f)
            {
                Vector2 newPos = Vector2.MoveTowards(cur2D, tgt2D, step);

                // Keep inside walkable cells
                Node nextNode = grid.NodeFromWorldPoint(new Vector3(newPos.x, newPos.y, transform.position.z));
                if (nextNode != null && nextNode.walkable)
                    transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
                else
                    break; // stop charge if next step would be unwalkable
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset cooldown AFTER the charge completes
        chargeTimer = chargeCooldown;

        // Decide what to do next (resume chase if close, otherwise return)
        float distToTarget = (target != null) ? Vector2.Distance(transform.position, target.position) : float.MaxValue;
        if (target != null && distToTarget <= leashRadius) SetState(AIState.Chase);
        else if (homeSet)                                  SetState(AIState.Return);
        else                                               SetState(AIState.Idle);
    }
}
