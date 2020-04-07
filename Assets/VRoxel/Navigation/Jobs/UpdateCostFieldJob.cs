using Unity.Collections;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct UpdateCostFieldJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte> voxels;

        [WriteOnly]
        public NativeArray<byte> costField;

        public void Execute(int i)
        {
            
        }
    }
}