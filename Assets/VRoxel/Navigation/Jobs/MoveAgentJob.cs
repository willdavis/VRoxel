using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// move and rotate each agent towards their desired direction
    /// </summary>
    [BurstCompile]
    public struct MoveAgentJob : IJobParallelForTransform
    {
        /// <summary>
        /// the speed for all agents
        /// </summary>
        public float speed;

        /// <summary>
        /// the elapsed time since last frame
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// the desired direction for each agent
        /// </summary>
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