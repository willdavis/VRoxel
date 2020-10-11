using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// A collection of functions to help with editing a voxel world
    /// </summary>
    public class WorldEditor
    {
        /// <summary>
        /// Set the block adjacent to the hit position in the voxel world.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The new block index to set</param>
        public static void AddBlock(World world, RaycastHit hit, byte block)
        {
            Vector3 position = world.AdjustRaycastHit(hit, Cube.Point.Outside);
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Update the block at the hit position in the voxel world
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The new block index to set</param>
        public static void ReplaceBlock(World world, RaycastHit hit, byte block)
        {
            Vector3 position = world.AdjustRaycastHit(hit, Cube.Point.Inside);
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Updates a block in the voxel world and flags the chunk as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="position">A position in the scene</param>
        /// <param name="block">The new block index to set</param>
        public static void SetBlock(World world, Vector3 position, byte block)
        {
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Updates a rectangle of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="start">The start position of the rectangle in the scene</param>
        /// <param name="end">The end position of the rectangle in the scene</param>
        /// <param name="block">The new block index to set</param>
        public static void SetRectangle(World world, Vector3 start, Vector3 end, byte block)
        {
            Vector3Int startPoint = world.SceneToGrid(start);
            Vector3Int endPoint = world.SceneToGrid(end);

            Vector3Int delta = Vector3Int.zero;
            delta.x = Mathf.Abs(endPoint.x - startPoint.x) + 1;
            delta.y = Mathf.Abs(endPoint.y - startPoint.y) + 1;
            delta.z = Mathf.Abs(endPoint.z - startPoint.z) + 1;

            Vector3Int min = Vector3Int.zero;
            min.x = Mathf.Min(startPoint.x, endPoint.x);
            min.y = Mathf.Min(startPoint.y, endPoint.y);
            min.z = Mathf.Min(startPoint.z, endPoint.z);

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
                        world.Write(point, block);
                    }
                }
            }
        }

        /// <summary>
        /// Updates blocks in the voxel world using Moore neighborhoods and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="position">The center of the neighborhood</param>
        /// <param name="range">The Moore neighborhood size</param>
        /// <param name="block">The new block index to set</param>
        public static void SetNeighborhood(World world, Vector3 position, int range, byte block)
        {
            int neighborhood = 2 * range + 1;
            Vector3Int offset = Vector3Int.zero;
            Vector3Int point = world.SceneToGrid(position);

            for (int i = 0; i < neighborhood; i++) // x-axis
            {
                offset.x = point.x + i - range;
                for (int j = 0; j < neighborhood; j++) // z-axis
                {
                    offset.z = point.z + j - range;
                    for (int k = 0; k < neighborhood; k++) // y-axis
                    {
                        offset.y = point.y + k - range;
                        world.Write(offset, block);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a sphere of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="block">The new block index to set</param>
        public static void SetSphere(World world, Vector3 position, float radius, byte block)
        {
            int size = Mathf.CeilToInt(radius);
            Vector3Int offset = Vector3Int.zero;
            Vector3Int point = world.SceneToGrid(position);

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
                            world.Write(offset, block);
                        }
                    }
                }
            }
        }
    }
}
