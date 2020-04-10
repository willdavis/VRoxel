using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Resets the integration field
    /// </summary>
    [BurstCompile]
    public struct ClearIntFieldJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<ushort> intField;

        public void Execute(int i)
        {
            intField[i] = ushort.MaxValue;
        }
    }
}