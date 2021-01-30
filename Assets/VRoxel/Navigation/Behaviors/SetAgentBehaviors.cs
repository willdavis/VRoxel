using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Changes the navigation behaviors for one or more agents
    /// </summary>
    [BurstCompile]
    public struct SetAgentBehaviors : IJobParallelFor
    {
        /// <summary>
        /// The new behaviors flags to set for each agent
        /// </summary>
        public AgentBehaviors flags;

        /// <summary>
        /// The list of agent navigation behaviors to be updated
        /// </summary>
        [WriteOnly] public NativeSlice<AgentBehaviors> behaviors;

        public void Execute(int i)
        {
            behaviors[i] = flags;
        }
    }
}