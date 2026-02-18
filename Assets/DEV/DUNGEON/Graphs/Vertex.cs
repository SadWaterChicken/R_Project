using UnityEngine;

public class Vertex
{
    public Vector2 Position { get; set; }

    public Vertex(Vector2 position)
    {
        Position = position;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vertex v)
        {
            return Position == v.Position;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public static bool operator ==(Vertex left, Vertex right)
    {
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return ReferenceEquals(left, right);
        return left.Position == right.Position;
    }

    public static bool operator !=(Vertex left, Vertex right)
    {
        return !(left == right);
    }
}
