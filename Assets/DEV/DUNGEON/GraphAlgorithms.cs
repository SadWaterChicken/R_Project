using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Graph algorithms for dungeon generation: Gabriel Graph and MST
/// </summary>
public static class GraphAlgorithms
{
    /// <summary>
    /// Create Relative Neighborhood Graph from nodes
    /// An edge exists if no other node is closer to both endpoints
    /// Only allows horizontal or vertical connections (no diagonals)
    /// </summary>
    public static void CreateRelativeNeighborhoodGraph(List<DungeonNode> nodes)
    {
        // Clear existing connections
        foreach (var node in nodes)
            node.Connections.Clear();

        // Check each pair of nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                DungeonNode a = nodes[i];
                DungeonNode b = nodes[j];

                // Only allow horizontal or vertical connections
                if (!IsAlignedConnection(a, b))
                    continue;

                if (IsRelativeNeighborhoodEdge(a, b, nodes))
                {
                    a.AddConnection(b);
                    b.AddConnection(a);
                }
            }
        }
    }

    private static bool IsRelativeNeighborhoodEdge(DungeonNode a, DungeonNode b, List<DungeonNode> allNodes)
    {
        float distAB = a.DistanceTo(b);

        // Check if any other node is in the "lune" region
        // For RNG: edge exists if no node c where max(d(a,c), d(b,c)) < d(a,b)
        foreach (var node in allNodes)
        {
            if (node == a || node == b)
                continue;

            float distAC = a.DistanceTo(node);
            float distBC = b.DistanceTo(node);
            
            // If node is closer to both a and b than a and b are to each other
            if (Mathf.Max(distAC, distBC) < distAB)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Check if two nodes are aligned horizontally or vertically (no diagonals)
    /// </summary>
    private static bool IsAlignedConnection(DungeonNode a, DungeonNode b)
    {
        float deltaX = Mathf.Abs(a.Position.x - b.Position.x);
        float deltaZ = Mathf.Abs(a.Position.z - b.Position.z);
        
        // Allow small tolerance for floating point errors
        float tolerance = 0.1f;
        
        // Either X must be nearly zero (vertical alignment) or Z must be nearly zero (horizontal alignment)
        return (deltaX < tolerance) || (deltaZ < tolerance);
    }

    /// <summary>
    /// Apply Minimal Spanning Tree using Kruskal's algorithm
    /// Ensures all nodes are connected in a single component
    /// </summary>
    public static void ApplyMinimalSpanningTree(List<DungeonNode> nodes)
    {
        // First, collect all edges from current connections
        List<Edge> edges = new List<Edge>();
        HashSet<(DungeonNode, DungeonNode)> processedPairs = new HashSet<(DungeonNode, DungeonNode)>();

        foreach (var node in nodes)
        {
            foreach (var connection in node.Connections)
            {
                var pair1 = (node, connection);
                var pair2 = (connection, node);
                
                if (!processedPairs.Contains(pair1) && !processedPairs.Contains(pair2))
                {
                    edges.Add(new Edge(node, connection, node.DistanceTo(connection)));
                    processedPairs.Add(pair1);
                }
            }
        }

        Debug.Log($"RNG created {edges.Count} edges");

        // Add ALL possible horizontal/vertical edges to ensure connectivity
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                DungeonNode a = nodes[i];
                DungeonNode b = nodes[j];

                if (IsAlignedConnection(a, b))
                {
                    var pair1 = (a, b);
                    var pair2 = (b, a);
                    
                    if (!processedPairs.Contains(pair1) && !processedPairs.Contains(pair2))
                    {
                        edges.Add(new Edge(a, b, a.DistanceTo(b)));
                        processedPairs.Add(pair1);
                    }
                }
            }
        }

        Debug.Log($"Total {edges.Count} possible edges for MST");

        // Sort edges by weight (distance)
        edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

        // Clear all connections
        foreach (var node in nodes)
            node.Connections.Clear();

        // Union-Find for cycle detection
        Dictionary<DungeonNode, DungeonNode> parent = new Dictionary<DungeonNode, DungeonNode>();
        foreach (var node in nodes)
            parent[node] = node;

        int edgesAdded = 0;
        int targetEdges = nodes.Count - 1; // MST needs n-1 edges for n nodes

        // Kruskal's algorithm
        foreach (var edge in edges)
        {
            DungeonNode rootA = Find(edge.NodeA, parent);
            DungeonNode rootB = Find(edge.NodeB, parent);

            if (rootA != rootB)
            {
                // Add edge to MST
                edge.NodeA.AddConnection(edge.NodeB);
                edge.NodeB.AddConnection(edge.NodeA);

                // Union
                parent[rootB] = rootA;
                edgesAdded++;

                if (edgesAdded >= targetEdges)
                    break;
            }
        }

        Debug.Log($"MST created with {edgesAdded} edges for {nodes.Count} nodes (target: {targetEdges})");
    }

    private static DungeonNode Find(DungeonNode node, Dictionary<DungeonNode, DungeonNode> parent)
    {
        if (parent[node] != node)
            parent[node] = Find(parent[node], parent);
        return parent[node];
    }

    /// <summary>
    /// Add some random edges back to create loops (optional, for more interesting dungeons)
    /// </summary>
    public static void AddRandomLoops(List<DungeonNode> nodes, int loopCount, List<Edge> availableEdges)
    {
        // Get edges that aren't currently in use
        HashSet<(DungeonNode, DungeonNode)> currentEdges = new HashSet<(DungeonNode, DungeonNode)>();
        foreach (var node in nodes)
        {
            foreach (var conn in node.Connections)
            {
                currentEdges.Add((node, conn));
                currentEdges.Add((conn, node));
            }
        }

        List<Edge> unusedEdges = availableEdges.Where(e => 
            !currentEdges.Contains((e.NodeA, e.NodeB)) && 
            !currentEdges.Contains((e.NodeB, e.NodeA))).ToList();

        // Add random loops
        for (int i = 0; i < loopCount && unusedEdges.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, unusedEdges.Count);
            Edge edge = unusedEdges[randomIndex];
            
            edge.NodeA.AddConnection(edge.NodeB);
            edge.NodeB.AddConnection(edge.NodeA);
            
            unusedEdges.RemoveAt(randomIndex);
        }
    }

    public class Edge
    {
        public DungeonNode NodeA;
        public DungeonNode NodeB;
        public float Weight;

        public Edge(DungeonNode a, DungeonNode b, float weight)
        {
            NodeA = a;
            NodeB = b;
            Weight = weight;
        }
    }
}
