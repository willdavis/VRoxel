using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Changes an agents navigation activity
    /// </summary>
    [BurstCompile]
    public struct ActivateAgents : IJobParallelFor
    {
        public bool status;

        [ReadOnly]
        public NativeSlice<bool> agents;

        public void Execute(int i)
        {
            agents[i] = status;
        }
    }
}