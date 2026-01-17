using Godot;
using System.Collections.Generic;

/// <summary>
/// Static utilities for odd-q hexagonal coordinate math.
/// Uses pointy-top hexagons with odd-q offset coordinates.
/// </summary>
public static class HexCoordinates
{
    public const float HexSize = 5f; // Radius (center to corner)
    public static readonly float HexWidth = HexSize * 2f;
    public static readonly float HexHeight = Mathf.Sqrt(3f) * HexSize;

    // Neighbor offsets for odd columns (shifted down in world space)
    private static readonly Vector2I[] OddNeighbors = new Vector2I[]
    {
        new(1, 0),   // Right-down
        new(1, 1),   // Right-up
        new(0, 1),   // Up
        new(-1, 1),  // Left-up
        new(-1, 0),  // Left-down
        new(0, -1)   // Down
    };

    // Neighbor offsets for even columns
    private static readonly Vector2I[] EvenNeighbors = new Vector2I[]
    {
        new(1, -1),  // Right-down
        new(1, 0),   // Right-up
        new(0, 1),   // Up
        new(-1, 0),  // Left-up
        new(-1, -1), // Left-down
        new(0, -1)   // Down
    };

    /// <summary>
    /// Convert hex coordinates to world position (Y = 0).
    /// </summary>
    public static Vector3 HexToWorld(Vector2I hex)
    {
        float x = hex.X * HexWidth * 0.75f;
        float z = hex.Y * HexHeight + (IsOddColumn(hex.X) ? HexHeight * 0.5f : 0f);
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Convert world position to hex coordinates.
    /// </summary>
    public static Vector2I WorldToHex(Vector3 worldPos)
    {
        // Approximate column
        int q = Mathf.RoundToInt(worldPos.X / (HexWidth * 0.75f));

        // Adjust row based on column parity
        float zOffset = IsOddColumn(q) ? HexHeight * 0.5f : 0f;
        int r = Mathf.RoundToInt((worldPos.Z - zOffset) / HexHeight);

        return new Vector2I(q, r);
    }

    /// <summary>
    /// Check if column is odd (works for negative numbers too).
    /// </summary>
    private static bool IsOddColumn(int col)
    {
        return (col & 1) != 0; // Bitwise AND with 1 works for negatives
    }

    /// <summary>
    /// Get all 6 neighboring hex coordinates.
    /// </summary>
    public static IEnumerable<Vector2I> GetNeighbors(Vector2I hex)
    {
        var offsets = IsOddColumn(hex.X) ? OddNeighbors : EvenNeighbors;
        foreach (var offset in offsets)
        {
            yield return hex + offset;
        }
    }

    /// <summary>
    /// Calculate distance between two hexes in hex steps.
    /// </summary>
    public static int HexDistance(Vector2I a, Vector2I b)
    {
        // Convert to cube coordinates for distance calculation
        var cubeA = OffsetToCube(a);
        var cubeB = OffsetToCube(b);

        return (Mathf.Abs(cubeA.X - cubeB.X) +
                Mathf.Abs(cubeA.Y - cubeB.Y) +
                Mathf.Abs(cubeA.Z - cubeB.Z)) / 2;
    }

    /// <summary>
    /// Convert odd-q offset to cube coordinates.
    /// </summary>
    private static Vector3I OffsetToCube(Vector2I hex)
    {
        int x = hex.X;
        int z = hex.Y - (hex.X - (hex.X & 1)) / 2;
        int y = -x - z;
        return new Vector3I(x, y, z);
    }

    /// <summary>
    /// Generate vertices for a flat hex mesh (6 triangles from center).
    /// </summary>
    public static Vector3[] GetHexVertices(float size)
    {
        var vertices = new List<Vector3>();
        vertices.Add(Vector3.Zero); // Center

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.DegToRad(60f * i); // Pointy-top orientation
            float x = size * Mathf.Cos(angle);
            float z = size * Mathf.Sin(angle);
            vertices.Add(new Vector3(x, 0f, z));
        }

        return vertices.ToArray();
    }

    /// <summary>
    /// Generate triangle indices for hex mesh (6 triangles).
    /// </summary>
    public static int[] GetHexIndices()
    {
        return new int[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 1
        };
    }
}
