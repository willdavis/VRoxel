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
        /// the turning speed for all agents
        /// </summary>
        public float turnSpeed;

        /// <summary>
        /// the elapsed time since last frame
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// the desired direction for each agent
        /// </summary>
        [ReadOnly]
        public NativeArray<float3> directions;

        public NativeArray<float3> velocity;

        public void Execute(int i, TransformAccess transform)
        {
            float3 position = transform.position;
            quaternion look = quaternion.LookRotation(directions[i], new float3(0,1,0));

            transform.position = position + directions[i] * speed * deltaTime;
            transform.rotation = math.slerp(transform.rotation, look, turnSpeed * deltaTime);
        }

        /// <summary>
        /// limits the magnitude of a vector to the max length
        /// </summary>
        public float3 Clamp(float3 vector, float max)
        {
            float length = max / math.length(vector);
            if (length < 1f) { return vector * length; }
            else { return vector; }
        }
    }
}