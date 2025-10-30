using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour
{
    private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    private PathRequest currentPathRequest;
    private static PathRequestManager instance;
    private Pathfinding pathfinding;
    private bool isProcessingPath;

    private void Awake()
    {
        // singleton guard
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[PathRequestManager] Another instance exists - destroying duplicate on " + gameObject.name);
            Destroy(this);
            return;
        }
        instance = this;

        // prefer Pathfinding on the same GameObject, fallback to any in scene
        pathfinding = GetComponent<Pathfinding>() ?? FindObjectOfType<Pathfinding>();
        if (pathfinding == null)
            Debug.LogWarning("[PathRequestManager] No Pathfinding component found on or in scene during Awake.");
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        // Ensure there is an instance. If not, try to find one (handles callers running before Awake).
        if (instance == null)
        {
            instance = FindObjectOfType<PathRequestManager>();
            if (instance == null)
            {
                Debug.LogError("[PathRequestManager] RequestPath called but no PathRequestManager instance exists in the scene.");
                callback?.Invoke(new Vector3[0], false);
                return;
            }
        }

        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;

            // Ensure we have a Pathfinding reference
            if (pathfinding == null)
            {
                pathfinding = GetComponent<Pathfinding>() ?? FindObjectOfType<Pathfinding>();
            }

            if (pathfinding != null)
            {
                pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
            }
            else
            {
                Debug.LogError("[PathRequestManager] Cannot process path - no Pathfinding component available. Failing request.");
                // Fail this request gracefully and continue with next
                try { currentPathRequest.callback?.Invoke(new Vector3[0], false); } catch { }
                isProcessingPath = false;
                TryProcessNext();
            }
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        try { currentPathRequest.callback(path, success); } catch { }
        isProcessingPath = false;
        TryProcessNext();
    }

    private struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
