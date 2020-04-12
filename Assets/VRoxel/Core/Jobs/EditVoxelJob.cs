using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Core
{
    [BurstCompile]
    public struct EditVoxelJob : IJob
    {
        public byte block;
        public int3 size;
        public int3 start;
        public int3 end;

        [WriteOnly]
        public NativeArray<byte> voxels;

        public void Execute()
        {
            int3 delta = int3.zero;
            delta.x = math.abs(end.x - start.x) + 1;
            delta.y = math.abs(end.y - start.y) + 1;
            delta.z = math.abs(end.z - start.z) + 1;

            int3 min = int3.zero;
            min.x = math.min(start.x, end.x);
            min.y = math.min(start.y, end.y);
            min.z = math.min(start.z, end.z);

            int3 point = int3.zero;
            for (int x = min.x; x < min.x + delta.x; x++)
            {
                point.x = x;
                for (int z = min.z; z < min.z + delta.z; z++)
                {
                    point.z = z;
                    for (int y = min.y; y < min.y + delta.y; y++)
                    {
                        point.y = y;
                        if (OutOfBounds(point)) { continue; }
                        voxels[Flatten(point)] = block;
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
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Test if a point is outside the voxel grid
        /// </summary>
        public bool OutOfBounds(int3 point)
        {
            if (point.x < 0 || point.x >= size.x) { return true; }
            if (point.y < 0 || point.y >= size.y) { return true; }
            if (point.z < 0 || point.z >= size.z) { return true; }
            return false;
        }
    }
}