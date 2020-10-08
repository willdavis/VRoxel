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
        /// All voxels inside the rectangle will be set to this block index
        /// </summary>
        public byte block;

        /// <summary>
        /// The global position of this chunk
        /// </summary>
        public int3 chunkOffset;

        /// <summary>
        /// The dimensions of this chunk
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
        [WriteOnly] public NativeArray<byte> voxels;


        /// <summary>
        /// Iterate through the rectangle and update voxels in the chunk
        /// </summary>
        public void Execute()
        {
            int3 min = int3.zero;
            int3 delta = int3.zero;
            int3 localPos = int3.zero;
            int3 globalPos = int3.zero;

            // calculate min and delta of the rectangle so the
            // start and end positions orientation will not matter
            delta.x = math.abs(end.x - start.x) + 1;
            delta.y = math.abs(end.y - start.y) + 1;
            delta.z = math.abs(end.z - start.z) + 1;
            
            min.x = math.min(start.x, end.x);
            min.y = math.min(start.y, end.y);
            min.z = math.min(start.z, end.z);

            // skip any point that is outside the chunk
            // and update any voxel inside the rectangle
            for (int x = min.x; x < min.x + delta.x; x++)
            {
                globalPos.x = x;
                for (int z = min.z; z < min.z + delta.z; z++)
                {
                    globalPos.z = z;
                    for (int y = min.y; y < min.y + delta.y; y++)
                    {
                        globalPos.y = y;

                        if (OutOfChunk(globalPos))
                            continue;

                        localPos = globalPos - chunkOffset;
                        voxels[Flatten(localPos)] = block;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate an array index from an int3 (Vector3Int) point
        /// </summary>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * chunkSize.y * chunkSize.z) + (point.y * chunkSize.z) + point.z;
        }

        /// <summary>
        /// Test if a global position is outside the chunk
        /// </summary>
        public bool OutOfChunk(int3 point)
        {
            if (point.y < chunkOffset.y || point.y >= chunkOffset.y + chunkSize.y) { return true; }
            if (point.z < chunkOffset.z || point.z >= chunkOffset.z + chunkSize.z) { return true; }
            if (point.x < chunkOffset.x || point.x >= chunkOffset.x + chunkSize.x) { return true; }
            return false;
        }
    }
}