using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour 
{
    public Transform target;
    public float speed = 20f;
    public float turnDst = 5f;
    public float turnSpeed = 3f;
    public float pathUpdateInterval = 0.25f;

    private Path path;
    private PathRequestManager pathRequestManager;
    private Grid grid;  // reference to the A* Grid component (not the scene tile drawing Grid object)
    private Coroutine followCoroutine;
    private Coroutine updatePathCoroutine;

    private void Awake()
    {
        // find manager (may be assigned by its Awake already)
        pathRequestManager = FindObjectOfType<PathRequestManager>();
        // get Grid from the same A* GameObject (avoid collisions with other Grid types)
        grid = pathRequestManager != null ? pathRequestManager.GetComponent<Grid>() : FindObjectOfType<Grid>();
    }

    // Use Start instead of OnEnable to avoid racing with other objects' Awakes
    private void Start()
    {
        if (pathRequestManager == null)
        {
            Debug.LogError("[Unit] No PathRequestManager found in scene!");
            enabled = false;
            return;
        }

        if (grid == null)
        {
            Debug.LogError("[Unit] No Grid found in scene!");
            enabled = false;
            return;
        }

        if (target == null)
        {
            Debug.LogWarning($"[Unit] No target assigned to Unit {gameObject.name}!");
            return;
        }

        updatePathCoroutine = StartCoroutine(UpdatePathLoop());
    }

    private void OnDisable()
    {
        if (updatePathCoroutine != null)
            StopCoroutine(updatePathCoroutine);
        if (followCoroutine != null)
            StopCoroutine(followCoroutine);
    }

    IEnumerator UpdatePathLoop()
    {
        var wait = new WaitForSeconds(pathUpdateInterval);
        while (true)
        {
            if (target != null && grid != null && pathRequestManager != null)
            {
                Node unitNode = grid.NodeFromWorldPoint(transform.position);
                Node targetNode = grid.NodeFromWorldPoint(target.position);

                if (unitNode != null && targetNode != null && unitNode.walkable && targetNode.walkable)
                {
                    Vector3 startTilePos = unitNode.worldPosition;
                    Vector3 targetTilePos = targetNode.worldPosition;

                    // Use static facade (safe) — PathRequestManager.RequestPath handles instance discovery
                    PathRequestManager.RequestPath(startTilePos, targetTilePos, OnPathFound);

                    Debug.DrawLine(startTilePos, targetTilePos, Color.yellow, pathUpdateInterval);
                }
            }
            yield return wait;
        }
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDst);
            
            if (followCoroutine != null)
                StopCoroutine(followCoroutine);
            followCoroutine = StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        if (path == null || path.lookPoints == null || path.lookPoints.Length == 0) yield break;

        int pathIndex = 0;
        Vector3 currentWaypoint = path.lookPoints[pathIndex];

        while (true)
        {
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(currentWaypoint.x, currentWaypoint.y);

            float step = speed * Time.deltaTime;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, step);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            if (Vector2.Distance(newPos, targetPos) < 0.05f)
            {
                pathIndex++;
                if (pathIndex >= path.lookPoints.Length) yield break;
                currentWaypoint = path.lookPoints[pathIndex];
            }

            yield return null;
        }
    }
}
