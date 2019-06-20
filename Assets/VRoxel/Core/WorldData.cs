using UnityEngine;

/// <summary>
/// Contains the voxel data for a World and exposes methods to modify the data
/// </summary>
public class WorldData
{
    private byte[,,] _cache;
    private Vector3 _center;

    public WorldData(Vector3Int size)
    {
        _cache = new byte[size.x, size.y, size.z];
        _center = new Vector3(size.x / 2f, size.y / 2f, size.z / 2f);
    }

    /// <summary>
    /// The center point of the voxel grid
    /// </summary>
    public Vector3 Center() { return _center; }

    /// <summary>
    /// Checks if the point is inside the bounds of the world data
    /// </summary>
    /// <param name="point">The point to compare</param>
    public bool Contains(Vector3Int point)
    {
        if (point.x < 0 || point.x >= _cache.GetLength(0)) { return false; }
        if (point.y < 0 || point.y >= _cache.GetLength(1)) { return false; }
        if (point.z < 0 || point.z >= _cache.GetLength(2)) { return false; }
        return true;
    }

    /// <summary>
    /// Get a block type from the world data cache
    /// </summary>
    /// <param name="point">A point inside the world</param>
    public byte Get(Vector3Int point) { return _cache[point.x, point.y, point.z]; }

    /// <summary>
    /// Set a block type in the world data cache
    /// </summary>
    /// <param name="point">A point inside the world</param>
    /// <param name="block">The block type to set in the world</param>
    public void Set(Vector3Int point, byte block) { _cache[point.x, point.y, point.z] = block; }
}
