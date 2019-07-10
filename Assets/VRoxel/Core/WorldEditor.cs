using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEditor
{
    /// <summary>
    /// Adjusts a RaycastHit point to be inside or outside of the block that it hit
    /// </summary>
    /// <param name="world">A reference to the World</param>
    /// <param name="hit">The RaycastHit to be adjusted</param>
    /// <param name="dir">choose inside or outside the cube</param>
    public static Vector3 Adjust(World world, RaycastHit hit, Cube.Point dir)
    {
        Vector3 position = hit.point;
        switch (dir)
        {
            case Cube.Point.Inside:
                position += hit.normal * (world.scale / -2f);
                break;
            case Cube.Point.Outside:
                position += hit.normal * (world.scale / 2f);
                break;
        }
        return position;
    }

    /// <summary>
    /// Calculates the voxel grid point for a position in the scene
    /// </summary>
    /// <param name="world">A reference to the World</param>
    /// <param name="position">A position in the scene</param>
    public static Vector3Int Get(World world, Vector3 position)
    {
        Vector3 adjusted = position;
        Vector3Int point = Vector3Int.zero;
        Quaternion rotation = Quaternion.Inverse(world.transform.rotation);

        adjusted += world.transform.position * -1f;         // adjust for the worlds position
        adjusted *= 1 / world.scale;                        // adjust for the worlds scale
        adjusted = rotation * adjusted;                     // adjust for the worlds rotation
        adjusted += world.data.center;                      // adjust for the worlds center

        point.x = Mathf.FloorToInt(adjusted.x);
        point.y = Mathf.FloorToInt(adjusted.y);
        point.z = Mathf.FloorToInt(adjusted.z);

        return point;
    }

    /// <summary>
    /// Calculates the scene position for a point in the voxel grid
    /// </summary>
    /// <param name="world">A reference to the World</param>
    /// <param name="point">A point in the voxel grid</param>
    public static Vector3 Get(World world, Vector3Int point)
    {
        Vector3 position = point;
        position += world.data.center * -1f;               // adjust for the worlds center
        position = world.transform.rotation * position;    // adjust for the worlds rotation
        position *= world.scale;                           // adjust for the worlds scale
        position += world.transform.position;              // adjust for the worlds position
        return position;
    }

    public static void Set(World world, Vector3Int point, byte block)
    {
        if (!world.data.Contains(point)) { return; }
        if (world.data.Get(point) == block) { return; }

        world.data.Set(point, block);
        world.chunks.UpdateFrom(point);
    }

    /// <summary>
    /// Safely set a range of blocks in the voxel grid
    /// </summary>
    /// <param name="world">A reference to the World</param>
    /// <param name="start">A point in the voxel grid</param>
    /// <param name="end">A point in the voxel grid</param>
    /// <param name="block">The block index to set</param>
    public static void Set(World world, Vector3Int start, Vector3Int end, byte block)
    {
        Vector3Int delta = Vector3Int.zero;
        delta.x = Mathf.Abs(end.x - start.x) + 1;
        delta.y = Mathf.Abs(end.y - start.y) + 1;
        delta.z = Mathf.Abs(end.z - start.z) + 1;

        Vector3Int min = Vector3Int.zero;
        min.x = Mathf.Min(start.x, end.x);
        min.y = Mathf.Min(start.y, end.y);
        min.z = Mathf.Min(start.z, end.z);

        Vector3Int point = Vector3Int.zero;
        for (int x = min.x; x < min.x + delta.x; x++)
        {
            point.x = x;
            for (int z = min.z; z < min.z + delta.z; z++)
            {
                point.z = z;
                for (int y = min.y; y < min.y + delta.y; y++)
                {
                    point.y = y;
                    Set(world, point, block);
                }
            }
        }
    }

    /// <summary>
    /// Safely set a Moore neighborhood of blocks in the voxel grid
    /// </summary>
    /// <param name="world">A reference to the World</param>
    /// <param name="point">A point in the voxel grid</param>
    /// <param name="range">The Moore neighborhood range</param>
    /// <param name="block">The block index to set</param>
    public static void Set(World world, Vector3Int point, int range, byte block)
    {
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
                    Set(world, offset, block);
                }
            }
        }
    }
}
