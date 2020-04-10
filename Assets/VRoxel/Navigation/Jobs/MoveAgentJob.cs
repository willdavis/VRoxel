using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct MoveAgentJob : IJobParallelForTransform
    {
        public float speed;
        public float deltaTime;

        [ReadOnly]
        public NativeArray<float3> directions;

        public void Execute(int i, TransformAccess transform)
        {
            if (math.length(directions[i]) == 0) { return; }

            float3 position = transform.position;
            transform.position = position + directions[i] * speed * deltaTime;
            transform.rotation = quaternion.LookRotation(directions[i], new float3(0,1,0));
        }
    }
}