using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Core.Chunks
{
    /// <summary>
    /// Modifies the voxels in a chunk within the bounds of a rectangle
    /// </summary>
    [BurstCompile]
    public struct ModifyRectangle : IJob
    {
        /// <summary>
        /// All voxels inside the rectangle will be set to this block
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
        /// The global position to start the rectangle
        /// </summary>
        public int3 start;

        /// <summary>
        /// The global position to end the rectangle
        /// </summary>
        public int3 end;

        /// <summary>
        /// The chunks voxel data that will be modified
        /// </summary>
        public NativeArray<byte> voxels;

        /// <summary>
        /// A reference to the block settings for the world
        /// </summary>
        [ReadOnly] public NativeArray<Block> blockLibrary;


        /// <summary>
        /// Iterate over the chunk and update voxels
        /// that are inside the rectangles bounds.
        /// </summary>
        public void Execute()
        {
            int3 min = int3.zero;
            int3 delta = int3.zero;
            int3 localPos = int3.zero;
            int3 globalPos = int3.zero;

            delta.x = math.abs(end.x - start.x) + 1;
            delta.y = math.abs(end.y - start.y) + 1;
            delta.z = math.abs(end.z - start.z) + 1;
            
            min.x = math.min(start.x, end.x);
            min.y = math.min(start.y, end.y);
            min.z = math.min(start.z, end.z);

            int index = 0;
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

                        index = Flatten(localPos);
                        if (NotEditable(voxels[index]))
                            continue;

                        voxels[index] = block;
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
        /// Checks if a voxel is not editable
        /// </summary>
        /// <param name="voxel">The voxel to test</param>
        public bool NotEditable(byte voxel)
        {
            return !blockLibrary[voxel].editable;
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
    }
}