using System;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using Priority_Queue;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Updates the integration field
    /// </summary>
    public struct UpdateIntFieldJob : IJob
    {
        /// <summary>
        /// the size of the integration field
        /// </summary>
        public int3 size;

        /// <summary>
        /// the goal where all paths will direct to
        /// </summary>
        public int3 goal;

        [ReadOnly]
        public NativeArray<int3> directions;

        [ReadOnly]
        public NativeArray<byte> costField;

        public NativeArray<ushort> intField;

        public void Execute()
        {
            if (OutOfBounds(goal)) { return; }

            int flatSize = size.x * size.y * size.z;
            NativeQueue<int3> open = new NativeQueue<int3>(Allocator.Temp);

            int flatIndex = Flatten(goal);
            open.Enqueue(goal);         // queue the goal position as the first open node
            intField[flatIndex] = 0;    // set the goal position integration cost to zero

            ushort cost;
            int index, nextIndex;
            int3 position, nextPosition;

            while (open.Count != 0)
            {
                position = open.Dequeue();
                index = Flatten(position);

                for (int i = 1; i < 27; i++)    // check neighbors
                {
                    nextPosition = position + directions[i];
                    if (OutOfBounds(nextPosition)) { continue; }

                    nextIndex = Flatten(nextPosition);
                    if (costField[nextIndex] == 255) { continue; }  // check for obstructed node

                    cost = Convert.ToUInt16(intField[index] + costField[nextIndex]);
                    if (intField[nextIndex] == ushort.MaxValue || cost < intField[nextIndex])
                    {
                        intField[nextIndex] = cost;
                        open.Enqueue(nextPosition);
                    }
                }
            }

            open.Dispose();
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