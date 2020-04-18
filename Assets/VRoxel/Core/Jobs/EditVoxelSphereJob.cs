using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Core
{
    [BurstCompile]
    public struct EditVoxelSphereJob : IJob
    {
        public byte block;
        public float radius;
        public int3 position;
        public int3 worldSize;

        [WriteOnly]
        public NativeArray<byte> voxels;

        public void Execute()
        {
            int size = (int)math.ceil(radius);
            int3 offset = int3.zero;

            for (int x = position.x - size; x <= position.x + size; x++)
            {
                offset.x = x;
                for (int z = position.z - size; z <= position.z + size; z++)
                {
                    offset.z = z;
                    for (int y = position.y - size; y <= position.y + size; y++)
                    {
                        offset.y = y;
                        if (OutOfBounds(offset)) { continue; }
                        if (math.distance(position, offset) <= radius)
                            voxels[Flatten(offset)] = block;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) point
        /// </summary>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * worldSize.y * worldSize.z) + (point.y * worldSize.z) + point.z;
        }

        /// <summary>
        /// Test if a point is outside the voxel grid
        /// </summary>
        public bool OutOfBounds(int3 point)
        {
            if (point.x < 0 || point.x >= worldSize.x) { return true; }
            if (point.y < 0 || point.y >= worldSize.y) { return true; }
            if (point.z < 0 || point.z >= worldSize.z) { return true; }
            return false;
        }
    }
}