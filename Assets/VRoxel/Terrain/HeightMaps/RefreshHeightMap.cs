using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

using VRoxel.Core.Chunks;

namespace VRoxel.Terrain.HeightMaps
{
    /// <summary>
    /// Updates the height of each (x,y) position in the height map
    /// </summary>
    [BurstCompile]
    public struct RefreshHeightMap : IJobParallelFor
    {
        /// <summary>
        /// The dimensions of the height map and voxels
        /// </summary>
        public int3 size;

        /// <summary>
        /// The metadata about each voxel terrain block
        /// </summary>
        [ReadOnly] public NativeArray<Block> blocks;

        /// <summary>
        /// The 3D array of voxel terrain blocks
        /// </summary>
        [ReadOnly] public NativeArray<byte> voxels;

        /// <summary>
        /// The 2D array of height map values
        /// </summary>
        [WriteOnly] public NativeArray<ushort> data;

        /// <summary>
        /// Scan the column from top to bottom looking for
        /// a solid block. Update the data with the height
        /// </summary>
        public void Execute(int i)
        {
            int2 point2D = UnFlatten(i);
            int3 point3D = new int3(point2D.x, 0, point2D.y);
            ushort height = ushort.MaxValue;

            for (int h = size.y; h > 0; h--)
            {
                point3D.y = h-1;
                if (Solid(point3D))
                {
                    height = (ushort)(h-1);
                    break;
                }
            }

            data[i] = height;
        }

        /// <summary>
        /// Test if the voxel is collidable
        /// </summary>
        public bool Solid(int3 point)
        {
            byte voxel = voxels[Flatten(point)];
            Block block = blocks[voxel];
            return block.collidable;
        }

        /// <summary>
        /// Calculate a voxel terrain index from an int3
        /// </summary>
        /// <param name="point">A point in the voxel terrain</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate int2 position from a height map index
        /// </summary>
        public int2 UnFlatten(int index)
        {
            int x = index / size.z;
            int z = index - x * size.z;
            return new int2(x,z);
        }
    }
}