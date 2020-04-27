using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct QueueBehavior : IJobParallelFor
    {
        public NativeArray<float3> steering;

        public void Execute(int i)
        {
            float3 direction = float3.zero;
            steering[i] += direction;
        }
    }
}