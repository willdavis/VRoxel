using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;

namespace VRoxel.Navigation
{
    public struct MoveAgentJob : IJobParallelForTransform
    {
        public float speed;
        public float deltaTime;

        [ReadOnly]
        public NativeArray<Vector3> directions;

        public void Execute(int i, TransformAccess transform)
        {
            if (directions[i].magnitude == 0) { return; }
            transform.position += directions[i] * speed * deltaTime;
            transform.rotation = Quaternion.LookRotation(directions[i], Vector3.up);
        }
    }
}