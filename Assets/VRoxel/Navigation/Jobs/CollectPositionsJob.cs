using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct CollectPositionsJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<float3> positions;

        public void Execute(int i, TransformAccess transform)
        {
            positions[i] = transform.position;
        }
    }
}