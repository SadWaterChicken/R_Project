using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;

// Disambiguate Debug to Unity's Debug (resolves conflict with System.Diagnostics.Debug)
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour
{
    private PathRequestManager requestManager;
    private Grid grid;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    private IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // Reset per-node transient state so stale gCost/parent don't break A*
        if (grid != null)
            grid.ResetNodeCosts();

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        if (grid == null)
        {
            Debug.LogWarning("Grid missing, cannot compute path.");
            yield break;
        }

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning($"FindPath: startNode or targetNode is null (startPos={startPos}, targetPos={targetPos})");
            yield break;
        }

        // Initialize start node costs properly
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = startNode;

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        // yield to avoid blocking a long frame (keeps behavior consistent with coroutine approach)
        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);

            // Validate waypoints - drop invalid points
            if (waypoints != null)
            {
                var valid = new List<Vector3>();
                foreach (var w in waypoints)
                {
                    if (!float.IsNaN(w.x) && !float.IsNaN(w.y) && !float.IsNaN(w.z))
                        valid.Add(w);
                }
                waypoints = valid.ToArray();
            }

            // Ensure final waypoint is the exact target position (so gizmo follows player precisely)
            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = new Vector3[] { targetPos };
            }
            else
            {
                // Replace the last waypoint (target node center) with the actual target position for visual fidelity.
                waypoints[waypoints.Length - 1] = targetPos;
            }

            // cache for drawing and consumers (if used)
            if (requestManager != null)
            {
                Path path = new Path(waypoints, startPos, 0.5f); // 0.5f turn distance tweakable
                requestManager.FinishedProcessingPath(path.lookPoints, pathSuccess);
            }
        }
        else
        {
            if (requestManager != null)
                requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        }
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        // Simply return all nodes in order
        List<Vector3> waypoints = new List<Vector3>();
        foreach (Node n in path)
        {
            waypoints.Add(n.worldPosition);
        }
        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
