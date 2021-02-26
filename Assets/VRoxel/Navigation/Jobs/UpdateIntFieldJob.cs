using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Builds an integration field around one or more target positions
    /// </summary>
    [BurstCompile]
    public struct UpdateIntFieldJob : IJob
    {
        /// <summary>
        /// the size of the integration field
        /// </summary>
        public int3 size;

        /// <summary>
        /// a list of target positions the field will point towards
        /// </summary>
        [ReadOnly] public NativeList<int3> targets;

        /// <summary>
        /// a reference to all 27 directions
        /// </summary>
        [ReadOnly] public NativeArray<int3> directions;

        /// <summary>
        /// the movement costs for each block in the world.
        /// Blocks with a value of 255 are obstructed.
        /// </summary>
        [ReadOnly] public NativeArray<byte> costField;

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
            open.Clear();

            // set each target position's integration cost to zero
            // and add each position to the frontier nodes (open list)
            for (int i = 0; i < targets.Length; i++)
            {
                if (OutOfBounds(targets[i]))
                    continue;

                int flatIndex = Flatten(targets[i]);
                open.Enqueue(targets[i]);
                intField[flatIndex] = 0;
            }

            // return if no target positions are valid
            if (open.Count == 0)
                return;

            ushort cost;
            int index, nextIndex;
            int3 position, nextPosition;

            NativeArray<int> mask = new NativeArray<int>(18, Allocator.Temp);
            for (int i = 0; i < 10; i++) // up, down, N, NE, E, SE, S, SW, W, NW
                mask[i] = i + 1;

            // top N, E, S, W
            mask[10] = 11;
            mask[11] = 13;
            mask[12] = 15;
            mask[13] = 17;

            // bottom N, E, S, W
            mask[14] = 19;
            mask[15] = 21;
            mask[16] = 23;
            mask[17] = 25;

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