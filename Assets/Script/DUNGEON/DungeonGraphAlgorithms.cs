using System.Collections.Generic;
using UnityEngine;

public static class DungeonGraphAlgorithms
{
    /// <summary>
    /// Prim's Algorithm để tạo Minimum Spanning Tree
    /// </summary>
    public static List<RoomConnection> PrimsAlgorithm(List<DungeonRoom> nodes, List<RoomConnection> allEdges)
    {
        List<RoomConnection> mst = new List<RoomConnection>();
        HashSet<DungeonRoom> visited = new HashSet<DungeonRoom>();
        
        if (nodes.Count == 0) return mst;

        visited.Add(nodes[0]);

        while (visited.Count < nodes.Count)
        {
            RoomConnection minEdge = null;
            float minDistance = float.MaxValue;

            // Tìm edge nhỏ nhất kết nối visited node với unvisited node
            foreach (var edge in allEdges)
            {
                bool aVisited = visited.Contains(edge.roomA);
                bool bVisited = visited.Contains(edge.roomB);

                // Edge phải kết nối visited và unvisited
                if ((aVisited && !bVisited) || (!aVisited && bVisited))
                {
                    if (edge.distance < minDistance)
                    {
                        minDistance = edge.distance;
                        minEdge = edge;
                    }
                }
            }

            if (minEdge == null) break;

            mst.Add(minEdge);
            visited.Add(minEdge.roomA);
            visited.Add(minEdge.roomB);
        }

        return mst;
    }

    /// <summary>
    /// Tính toán tất cả khoảng cách giữa các phòng
    /// </summary>
    public static List<RoomConnection> GetAllConnections(List<DungeonRoom> rooms)
    {
        List<RoomConnection> allConnections = new List<RoomConnection>();
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                allConnections.Add(new RoomConnection(rooms[i], rooms[j]));
            }
        }

        // Sắp xếp theo khoảng cách
        allConnections.Sort((a, b) => a.distance.CompareTo(b.distance));

        return allConnections;
    }

    /// <summary>
    /// Proper 2D Delaunay Triangulation using Bowyer-Watson algorithm
    /// Converts DungeonRoom positions to Vertex objects for triangulation
    /// </summary>
    public static List<RoomConnection> DelaunayTriangulate(List<DungeonRoom> rooms)
    {
        if (rooms.Count < 3)
        {
            Debug.LogWarning("Need at least 3 rooms for Delaunay triangulation");
            return new List<RoomConnection>();
        }

        // Convert room positions to Vertex objects (using X, Z plane for 2D)
        List<Vertex> vertices = new List<Vertex>();
        Dictionary<Vertex, DungeonRoom> vertexToRoom = new Dictionary<Vertex, DungeonRoom>();
        
        for (int i = 0; i < rooms.Count; i++)
        {
            Vertex v = new Vertex(new Vector2(rooms[i].Center.x, rooms[i].Center.z));
            vertices.Add(v);
            vertexToRoom[v] = rooms[i];
        }

        // Perform Delaunay triangulation
        Delaunay2D triangulation = Delaunay2D.Triangulate(vertices);

        if (triangulation == null || triangulation.Edges.Count == 0)
        {
            Debug.LogWarning("Delaunay triangulation failed");
            return new List<RoomConnection>();
        }

        // Convert edges back to RoomConnections
        List<RoomConnection> edges = new List<RoomConnection>();
        HashSet<(int, int)> addedEdges = new HashSet<(int, int)>();

        foreach (var edge in triangulation.Edges)
        {
            if (vertexToRoom.TryGetValue(edge.U, out DungeonRoom roomA) &&
                vertexToRoom.TryGetValue(edge.V, out DungeonRoom roomB))
            {
                int idxA = rooms.IndexOf(roomA);
                int idxB = rooms.IndexOf(roomB);
                int minIdx = Mathf.Min(idxA, idxB);
                int maxIdx = Mathf.Max(idxA, idxB);

                if (!addedEdges.Contains((minIdx, maxIdx)))
                {
                    edges.Add(new RoomConnection(roomA, roomB));
                    addedEdges.Add((minIdx, maxIdx));
                }
            }
        }

        return edges;
    }

    /// <summary>
    /// Complete workflow: Simple triangulation -> MST -> Optional edges (10% chance) -> Filter
    /// </summary>
    public static List<RoomConnection> GenerateDungeonConnections(List<DungeonRoom> rooms, float optionalEdgeChance = 0.1f)
    {
        // Step 1: Create simple triangulation edges
        List<RoomConnection> delaunayEdges = DelaunayTriangulate(rooms);
        
        if (delaunayEdges == null || delaunayEdges.Count == 0)
            return new List<RoomConnection>();

        // Step 2: Create MST from triangulation edges
        List<RoomConnection> mstEdges = PrimsAlgorithm(rooms, delaunayEdges);

        // Step 3: Mark MST edges and add optional edges with chance
        HashSet<RoomConnection> markedEdges = new HashSet<RoomConnection>(mstEdges);
        
        foreach (var edge in delaunayEdges)
        {
            if (!markedEdges.Contains(edge))
            {
                // 10% chance to add non-MST edges for extra connectivity
                if (Random.value < optionalEdgeChance)
                {
                    markedEdges.Add(edge);
                }
            }
        }

        return new List<RoomConnection>(markedEdges);
    }
}