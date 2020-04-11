using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Resets the integration field by
    /// setting all voxels to their max value
    /// </summary>
    [BurstCompile]
    public struct ClearIntFieldJob : IJobParallelFor
    {
        /// <summary>
        /// the integrated cost values for each block in the world.
        /// Blocks with a value of 65535 are obstructed.
        /// </summary>
        [WriteOnly]
        public NativeArray<ushort> intField;

        public void Execute(int i)
        {
            intField[i] = ushort.MaxValue;
        }
    }
}