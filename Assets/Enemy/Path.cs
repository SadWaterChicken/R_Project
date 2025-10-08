using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public readonly Vector3[] lookPoints;   // Waypoints for debug
    public readonly Line[] turnBoundaries;  // Turn detection lines
    public readonly int finishLineIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst)
    {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);

        for (int i = 0; i < lookPoints.Length; i++)
        {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;

            // Place turn boundary a bit before reaching the waypoint
            Vector2 turnBoundaryPoint = (i == finishLineIndex)
                ? currentPoint
                : currentPoint - dirToCurrentPoint * turnDst;

            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);

            previousPoint = currentPoint;
        }
    }

    private Vector2 V3ToV2(Vector3 v3)
    {
        // ✅ Important: use XY for 2D grid (not XZ like 3D nav)
        return new Vector2(v3.x, v3.y);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.forward * 0.1f, Vector3.one * 0.1f); // draw small cubes
        }

        Gizmos.color = Color.white;
        foreach (Line l in turnBoundaries)
        {
            l.DrawWithGizmos(1);
        }
    }
}
