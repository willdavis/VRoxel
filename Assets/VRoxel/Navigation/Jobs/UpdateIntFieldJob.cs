using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Updates the integration field around a goal position
    /// </summary>
    [BurstCompile]
    public struct UpdateIntFieldJob : IJob
    {
        /// <summary>
        /// the size of the integration field
        /// </summary>
        public int3 size;

        /// <summary>
        /// the goal where all paths lead to
        /// </summary>
        public int3 goal;

        /// <summary>
        /// a reference to all 27 directions
        /// </summary>
        [ReadOnly]
        public NativeArray<int3> directions;

        /// <summary>
        /// the movement costs for each block in the world.
        /// Blocks with a value of 255 are obstructed.
        /// </summary>
        [ReadOnly]
        public NativeArray<byte> costField;

        /// <summary>
        /// the integrated cost values for each block in the world.
        /// Blocks with a value of 65535 are obstructed.
        /// </summary>
        public NativeArray<ushort> intField;

        /// <summary>
        /// the frontier nodes in a Dijkstra or Breadth First Search
        /// </summary>
        public NativeQueue<int3> open;

        public void Execute()
        {
            if (OutOfBounds(goal)) { return; }

            int flatSize = size.x * size.y * size.z;
            int flatIndex = Flatten(goal);

            open.Clear();
            open.Enqueue(goal);         // queue the goal position as the first open node
            intField[flatIndex] = 0;    // set the goal position integration cost to zero

            ushort cost;
            int index, nextIndex;
            int3 position, nextPosition;

            NativeArray<int> mask = new NativeArray<int>(6, Allocator.Temp);
            mask[0] = 1;
            mask[1] = 2;
            mask[2] = 3;
            mask[3] = 5;
            mask[4] = 7;
            mask[5] = 9;

            while (open.Count != 0)
            {
                position = open.Dequeue();
                index = Flatten(position);

                for (int i = 0; i < mask.Length; i++)    // check neighbors
                {
                    nextPosition = position + directions[mask[i]];
                    if (OutOfBounds(nextPosition)) { continue; }    // node is out of bounds

                    nextIndex = Flatten(nextPosition);
                    if (costField[nextIndex] == 255) { continue; }  // node is obstructed

                    cost = Convert.ToUInt16(intField[index] + costField[nextIndex]);
                    if (intField[nextIndex] == ushort.MaxValue || cost < intField[nextIndex])
                    {
                        intField[nextIndex] = cost;
                        open.Enqueue(nextPosition);
                    }
                }
            }

            mask.Dispose();
        }

        /// <summary>
        /// Calculate an array index from an int3 (Vector3Int) position
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Test if a point is outside the flow field
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