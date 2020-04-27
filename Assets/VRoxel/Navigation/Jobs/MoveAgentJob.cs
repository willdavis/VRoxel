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
        public float maxSpeed;

        /// <summary>
        /// the turning speed for all agents
        /// </summary>
        public float turnSpeed;

        /// <summary>
        /// the elapsed time since last frame
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// the desired direction of each agent
        /// </summary>
        [ReadOnly]
        public NativeArray<float3> direction;

        /// <summary>
        /// the current velocity of each agent
        /// </summary>
        public NativeArray<float3> velocity;

        public void Execute(int i, TransformAccess transform)
        {
            float3 position = transform.position;
            float3 desired = direction[i] * maxSpeed;
            float3 steering = desired - velocity[i];

            velocity[i] = Clamp(velocity[i] + steering, maxSpeed);
            transform.position = position + velocity[i] * deltaTime;

            quaternion look = quaternion.LookRotation(direction[i], new float3(0,1,0));
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