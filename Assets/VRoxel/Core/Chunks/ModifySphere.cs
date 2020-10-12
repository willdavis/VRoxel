using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Core.Chunks
{
    /// <summary>
    /// Modifies the voxels in a chunk within the radius of a sphere
    /// </summary>
    [BurstCompile]
    public struct ModifySphere : IJob
    {
        /// <summary>
        /// All voxels inside the sphere will be set to this block
        /// </summary>
        public byte block;

        /// <summary>
        /// The global position offset for this chunk
        /// </summary>
        public int3 chunkOffset;

        /// <summary>
        /// The (x,y,z) dimensions of this chunk
        /// </summary>
        public int3 chunkSize;

        /// <summary>
        /// The global position for the center of the sphere
        /// </summary>
        public int3 center;

        /// <summary>
        /// The radius of the sphere
        /// </summary>
        public float radius;

        /// <summary>
        /// The chunks voxel data that will be modified
        /// </summary>
        [WriteOnly] public NativeArray<byte> voxels;


        /// <summary>
        /// Iterate over the chunk and update voxels
        /// that are inside the spheres radius
        /// </summary>
        public void Execute()
        {
            int3 min = int3.zero;
            int3 delta = int3.zero;
            int3 localPos = int3.zero;
            int3 globalPos = int3.zero;

            delta.x = (int)math.round(radius * radius) + 1;
            delta.y = (int)math.round(radius * radius) + 1;
            delta.z = (int)math.round(radius * radius) + 1;

            min.x = center.x - (int)math.round(radius);
            min.y = center.y - (int)math.round(radius);
            min.z = center.z - (int)math.round(radius);

            for (int x = 0; x < chunkSize.x; x++)
            {
                localPos.x = x;
                for (int z = 0; z < chunkSize.z; z++)
                {
                    localPos.z = z;
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        localPos.y = y;
                        globalPos = localPos + chunkOffset;

                        if (OutOfRectangle(globalPos, min, delta))
                            continue;
                        if (OutOfSphere(globalPos, center, radius))
                            continue;

                        voxels[Flatten(localPos)] = block;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate a chunk index from a local position in the chunk
        /// </summary>
        /// <param name="point">A local position in the chunk</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * chunkSize.y * chunkSize.z)
                + (point.y * chunkSize.z)
                + point.z;
        }

        /// <summary>
        /// Checks if a position is out side the bounds of the rectangle
        /// </summary>
        /// <param name="point">A global position in the voxel space</param>
        /// <param name="min">The global position to start the rectangle</param>
        /// <param name="delta">The height, width, and depth of the rectangle</param>
        public bool OutOfRectangle(int3 point, int3 min, int3 delta)
        {
            if (point.y < min.y || point.y >= min.y + delta.y) { return true; }
            if (point.z < min.z || point.z >= min.z + delta.z) { return true; }
            if (point.x < min.x || point.x >= min.x + delta.x) { return true; }
            return false;
        }

        /// <summary>
        /// Checks if a position is out side the radius of the sphere
        /// </summary>
        /// <param name="point">A global position in the voxel space</param>
        /// <param name="center">The global position for the center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        public bool OutOfSphere(int3 point, int3 center, float radius)
        {
            if (math.distance(point, center) <= radius)
                return false;

            return true;
        }
    }
}