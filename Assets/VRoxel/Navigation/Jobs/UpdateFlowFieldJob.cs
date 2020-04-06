using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct UpdateFlowFieldJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> intField;

        [ReadOnly]
        public NativeArray<Vector3Int> flowDirections;

        [WriteOnly]
        public NativeArray<byte> flowField;

        public void Execute(int i)
        {
            
        }
    }
}