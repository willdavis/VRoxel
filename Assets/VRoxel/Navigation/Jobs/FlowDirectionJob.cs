using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct FlowDirectionJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<Vector3> directions;

        public void Execute(int i, TransformAccess transform)
        {
            directions[i] = Vector3.up;
        }
    }
}