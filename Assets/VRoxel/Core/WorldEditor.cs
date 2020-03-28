using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    public class WorldEditor
    {
        /// <summary>
        /// Set the block adjacent to the hit position in the world.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The block index</param>
        public static void Add(World world, RaycastHit hit, byte block)
        {
            Vector3 position = Adjust(world, hit, Cube.Point.Outside);
            Vector3Int point = Get(world, position);
            Set(world, point, block);
        }

        /// <summary>
        /// Update the block at the hit position in the world
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The block index</param>
        public static void Replace(World world, RaycastHit hit, byte block)
        {
            Vector3 position = Adjust(world, hit, Cube.Point.Inside);
            Vector3Int point = Get(world, position);
            Set(world, point, block);
        }

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
            position += Vector3.one * 0.5f;                    // adjust for the chunks center
            position += world.data.center * -1f;               // adjust for the worlds center
            position = world.transform.rotation * position;    // adjust for the worlds rotation
            position *= world.scale;                           // adjust for the worlds scale
            position += world.transform.position;              // adjust for the worlds position
            return position;
        }

        /// <summary>
        /// Safely set a block in the world and flag it's Chunk as stale.
        /// If the block is on the edge of a Chunk, the adjacent Chunk will also be set as stale
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">A position in the scene</param>
        /// <param name="block">The block index</param>
        public static void Set(World world, Vector3 position, byte block)
        {
            Vector3Int point = Get(world, position);
            Set(world, point, block);
        }

        /// <summary>
        /// Safely set a range of blocks in the world using two positions in the scene.
        /// Any chunks that were modified will be flagged as stale.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="start">A position in the scene</param>
        /// <param name="end">A position in the scene</param>
        /// <param name="block">The block index to set</param>
        public static void Set(World world, Vector3 start, Vector3 end, byte block)
        {
            Vector3Int startPoint = Get(world, start);
            Vector3Int endPoint = Get(world, end);
            Set(world, startPoint, endPoint, block);
        }

        /// <summary>
        /// Safely set a Moore neighborhood of blocks in the world.
        /// Any chunks that were modified will be flagged as stale.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">A position in the scene</param>
        /// <param name="range">The Moore neighborhood size</param>
        /// <param name="block">The block index to set</param>
        public static void Set(World world, Vector3 position, int range, byte block)
        {
            Vector3Int point = Get(world, position);
            Set(world, point, range, block);
        }

        /// <summary>
        /// Safely set a sphere of blocks in the world.
        /// Any chunks that were modified will be flagged as stale.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">A position in the scene</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="block">The block index to set</param>
        public static void Set(World world, Vector3 position, float radius, byte block)
        {
            Vector3Int point = Get(world, position);
            Set(world, point, radius, block);
        }

        /// <summary>
        /// Safely set a block in the World and flag it's Chunk as stale.
        /// If the block is on the edge of a Chunk, the adjacent Chunk will also be set as stale
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="point">A point in the voxel grid</param>
        /// <param name="block">The block index</param>
        public static void Set(World world, Vector3Int point, byte block)
        {
            if (!world.data.Contains(point)) { return; }
            if (world.GetBlock(point).isStatic) { return; }
            if (world.data.Get(point.x, point.y, point.z) == block) { return; }

            world.data.Set(point.x, point.y, point.z, block);
            world.chunks.UpdateFrom(point);
        }

        /// <summary>
        /// Safely set a range of blocks in the World between a start and end point
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

        /// <summary>
        /// Safely set a sphere of blocks in the voxel grid
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="point">A point in the voxel grid</param>
        /// <param name="radius">The radius in the voxel grid</param>
        /// <param name="block">The block index to set</param>
        public static void Set(World world, Vector3Int point, float radius, byte block)
        {
            int size = Mathf.CeilToInt(radius);
            Vector3Int offset = Vector3Int.zero;

            for (int x = point.x - size; x <= point.x + size; x++)
            {
                offset.x = x;
                for (int z = point.z - size; z <= point.z + size; z++)
                {
                    offset.z = z;
                    for (int y = point.y - size; y <= point.y + size; y++)
                    {
                        offset.y = y;
                        if (Vector3Int.Distance(point, offset) <= radius)
                        {
                            Set(world, offset, block);
                        }
                    }
                }
            }
        }
    }
}
