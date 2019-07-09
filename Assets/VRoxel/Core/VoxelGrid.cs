using UnityEngine;

/// <summary>
/// Contains voxel data, configuration, and helper methods
/// </summary>
public class VoxelGrid
{
    private byte[,,] _cache;
    private Vector3 _center;
    private Vector3Int _size;

    public VoxelGrid(Vector3Int size)
    {
        _size = size;
        _cache = new byte[size.x, size.y, size.z];
        _center = new Vector3(size.x / 2f, size.y / 2f, size.z / 2f);
    }

    /// <summary>
    /// The center point of the voxel grid
    /// </summary>
    public Vector3 center { get { return _center; } }

    /// <summary>
    /// Tests if a point is inside the voxel grid
    /// </summary>
    /// <param name="point">The point to compare</param>
    public bool Contains(Vector3Int point)
    {
        if (point.x < 0 || point.x >= _size.x) { return false; }
        if (point.y < 0 || point.y >= _size.y) { return false; }
        if (point.z < 0 || point.z >= _size.z) { return false; }
        return true;
    }

    /// <summary>
    /// Unsafely get a block type from the voxel grid cache
    /// </summary>
    /// <param name="x">the X coordinate</param>
    /// <param name="y">the Y coordinate</param>
    /// <param name="z">the Z coordinate</param>
    public byte Get(int x, int y, int z) { return _cache[x, y, z]; }

    /// <summary>
    /// Safely get a block type from the voxel grid cache
    /// </summary>
    /// <param name="point">A point inside the world</param>
    public byte Get(Vector3Int point)
    {
        if (!Contains(point)) { return 0; }
        return Get(point.x, point.y, point.z);
    }

    /// <summary>
    /// Unsafely set a block index in the voxel grid cache
    /// </summary>
    /// <param name="x">the X coordinate</param>
    /// <param name="y">the Y coordinate</param>
    /// <param name="z">the Z coordinate</param>
    /// <param name="block">The block index to set</param>
    public void Set(int x, int y, int z, byte block) { _cache[x, y, z] = block; }

    /// <summary>
    /// Safely set a block in the voxel grid cache
    /// </summary>
    /// <param name="point">A point inside the voxel grid</param>
    /// <param name="block">The block index to set</param>
    public void Set(Vector3Int point, byte block)
    {
        if (!Contains(point)) { return; }
        Set(point.x, point.y, point.z, block);
    }

    /// <summary>
    /// Safely set a range of blocks in the voxel grid
    /// </summary>
    /// <param name="start">A point in the voxel grid</param>
    /// <param name="end">A point in the voxel grid</param>
    /// <param name="block">The block index to set</param>
    public void Set(Vector3Int start, Vector3Int end, byte block)
    {
        if (!Contains(start)) { return; }
        if (!Contains(end)) { return; }

        Vector3Int delta = Vector3Int.zero;
        delta.x = Mathf.Abs(end.x - start.x) + 1;
        delta.y = Mathf.Abs(end.y - start.y) + 1;
        delta.z = Mathf.Abs(end.z - start.z) + 1;

        Vector3Int min = Vector3Int.zero;
        min.x = Mathf.Min(start.x, end.x);
        min.y = Mathf.Min(start.y, end.y);
        min.z = Mathf.Min(start.z, end.z);

        for (int x = min.x; x < min.x + delta.x; x++)
        {
            for (int z = min.z; z < min.z + delta.z; z++)
            {
                for (int y = min.y; y < min.y + delta.y; y++)
                {
                    Set(x, y, z, block);
                }
            }
        }
    }

    /// <summary>
    /// Safely set a Moore neighborhood of blocks in the voxel grid
    /// </summary>
    /// <param name="start">A point in the voxel grid</param>
    /// <param name="range">The Moore neighborhood range</param>
    /// <param name="block">The block index to set</param>
    /// range = 0, 1^3 block neighborhood
    /// range = 1, 3^3 block neighborhood
    /// range = 2, 5^3 block neighborhood
    /// range = 3, 7^3 block neighborhood
    public void Set(Vector3Int point, int range, byte block)
    {
        if (!Contains(point)) { return; }

        int neighborhood = 2 * range + 1;
        Vector3Int offset = Vector3Int.zero;

        for (int i = 0; i < neighborhood; i++) // x-axis
        {
            offset.x = point.x + i - range;
            for (int j = 0; j < neighborhood; j++) // z-axis
            {
                offset.z = point.z + j - range;
                for (int k = 0; k < neighborhood; k++) // y-axis
                {
                    offset.y = point.y + k - range;
                    Set(offset, block);
                }
            }
        }
    }
}
