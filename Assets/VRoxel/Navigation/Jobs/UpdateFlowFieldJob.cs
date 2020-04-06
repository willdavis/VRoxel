using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct UpdateFlowFieldJob : IJobParallelFor
    {
        public Vector3Int size;

        [ReadOnly]
        public NativeArray<int> intField;

        [ReadOnly]
        public NativeArray<Vector3Int> flowDirections;

        [WriteOnly]
        public NativeArray<byte> flowField;

        public void Execute(int i)
        {
            
        }

        /// <summary>
        /// Calculate a 1D array index from a Vector3Int position
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public int Flatten(Vector3Int point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate a Vector3Int from a 1D array index
        /// </summary>
        public Vector3Int UnFlatten(int index)
        {
            int x = index / (size.y * size.z);
            int y = (index - x * size.y * size.z) / size.z;
            int z = index - x * size.y * size.z - y * size.z;
            return new Vector3Int(x,y,z);
        }
    }
}