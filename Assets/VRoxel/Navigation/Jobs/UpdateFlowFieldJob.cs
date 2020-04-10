using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct UpdateFlowFieldJob : IJobParallelFor
    {
        /// <summary>
        /// the size of the flow field
        /// </summary>
        public int3 size;

        [ReadOnly]
        public NativeArray<int3> directions;

        [ReadOnly]
        public NativeArray<ushort> intField;

        [WriteOnly]
        public NativeArray<byte> flowField;

        public void Execute(int i)
        {
            int3 currentPoint = UnFlatten(i);
            int3 nextPoint = int3.zero;

            int lowest = intField[i];
            int direction = 0;
            int nextIndex;

            // find the best direction by comparing neighbors
            // with the lowest integration value
            for (int d = 1; d < 27; d++)
            {
                nextPoint = currentPoint + directions[d];
                if (OutOfBounds(nextPoint)) { continue; }

                nextIndex = Flatten(nextPoint);
                if (intField[nextIndex] < lowest)
                {
                    lowest = intField[nextIndex];
                    direction = d;
                }
            }

            // update the flow field with the best direction
            // if no direction was found, it will default to 0
            flowField[i] = (byte)direction;
        }

        /// <summary>
        /// Calculate an array index from a Vector3Int point
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate a Vector3Int point from an array index
        /// </summary>
        public int3 UnFlatten(int index)
        {
            int x = index / (size.y * size.z);
            int y = (index - x * size.y * size.z) / size.z;
            int z = index - x * size.y * size.z - y * size.z;
            return new int3(x,y,z);
        }

        /// <summary>
        /// Test if a point is inside the flow field
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public bool OutOfBounds(int3 point)
        {
            if (point.x < 0 || point.x >= size.x) { return true; }
            if (point.y < 0 || point.y >= size.y) { return true; }
            if (point.z < 0 || point.z >= size.z) { return true; }
            return false;
        }
    }
}