using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A navigation behavior that applies a braking force to stop an agent's movement
    /// </summary>
    [BurstCompile]
    public struct StoppingBehavior : IJobParallelFor
    {
        /// <summary>
        /// the force that will be applied to stop the agent's movement
        /// </summary>
        public float maxBrakeForce;

        /// <summary>
        /// the current steering forces acting on each agent
        /// </summary>
        public NativeArray<float3> steering;

        /// <summary>
        /// the navigation behaviors of each agent
        /// </summary>
        [ReadOnly] public NativeArray<AgentBehaviors> behaviors;

        /// <summary>
        /// the position and velocity of each agent
        /// </summary>
        [ReadOnly] public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// Reverse the agents current velocity and
        /// apply the braking force to slow them down
        /// </summary>
        public void ApplyBrakeForce(int i)
        {
            steering[i] += -steering[i] * maxBrakeForce;
            steering[i] += -agents[i].velocity;
        }

        public void Execute(int i)
        {
            AgentBehaviors mask = AgentBehaviors.Stopping;
            if ((behaviors[i] & mask) == 0) { return; }

            ApplyBrakeForce(i);
        }
    }
}