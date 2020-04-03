using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct FlowDirectionJob : IJobParallelForTransform
    {
        public float world_scale;
        public Vector3 world_offset;
        public Vector3 world_center;
        public Quaternion world_rotation;

        [WriteOnly]
        public NativeArray<Vector3> directions;

        public void Execute(int i, TransformAccess transform)
        {
            directions[i] = Vector3.up;
        }
    }
}