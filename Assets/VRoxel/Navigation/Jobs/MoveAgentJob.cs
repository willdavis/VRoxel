using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct MoveAgentJob : IJobParallelForTransform
    {
        /// <summary>
        /// the elapsed time since last frame
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// the maximum steering force that can be applied to an agent
        /// </summary>
        public float maxForce;

        /// <summary>
        /// the max speed of all agents
        /// </summary>
        public float maxSpeed;

        /// <summary>
        /// the turning speed of all agents
        /// </summary>
        public float turnSpeed;

        /// <summary>
        /// the mass of all agents
        /// </summary>
        public float mass;

        /// <summary>
        /// the current steering forces applied to each agent
        /// </summary>
        public NativeArray<float3> steering;

        /// <summary>
        /// the current velocity of each agent
        /// </summary>
        public NativeArray<float3> velocity;

        public void Execute(int i, TransformAccess transform)
        {
            float3 up = new float3(0,1,0);
            float3 position = transform.position;
            quaternion rotation = transform.rotation;

            steering[i] = Clamp(steering[i], maxForce);
            steering[i] = steering[i] / mass;

            velocity[i] = Clamp(velocity[i] + steering[i], maxSpeed);
            transform.position = position + velocity[i] * deltaTime;
            steering[i] = float3.zero;  // clear steering forces for the next frame

            if (velocity[i].Equals(float3.zero)) { return; }
            quaternion look = quaternion.LookRotation(velocity[i], up);
            transform.rotation = math.slerp(rotation, look, turnSpeed * deltaTime);
        }

        /// <summary>
        /// limits the magnitude of a vector to the given max length
        /// </summary>
        public float3 Clamp(float3 vector, float max)
        {
            float length = max / math.length(vector);
            if (length < 1f) { return vector * length; }
            else { return vector; }
        }
    }
}