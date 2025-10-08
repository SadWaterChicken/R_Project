using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;       // Move speed
    public float turnDst = 5f;      // How early to start turning
    public float turnSpeed = 3f;    // Rotation speed

    private Path path;

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path(waypoints, transform.position, turnDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (path == null) yield break;

        int pathIndex = 0;
        Vector3 currentWaypoint = path.lookPoints[pathIndex];

        while (true)
        {
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPos = new Vector2(currentWaypoint.x, currentWaypoint.y);

            // Move towards waypoint
            float step = speed * Time.deltaTime;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, step);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

            // Switch to next waypoint when close enough
            if (Vector2.Distance(newPos, targetPos) < 0.05f) // threshold tweakable
            {
                pathIndex++;
                if (pathIndex >= path.lookPoints.Length) yield break;
                currentWaypoint = path.lookPoints[pathIndex];
            }

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}
