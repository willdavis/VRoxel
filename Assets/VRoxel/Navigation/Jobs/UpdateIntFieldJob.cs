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
        /// the minimum cost difference between two nodes.
        /// Prevents overlap due to small cost differences
        /// </summary>
        public int minCostDiff;

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
        /// the frontier nodes in the integration field
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

                // check each neighboring node
                for (int i = 0; i < mask.Length; i++)
                {
                    nextPosition = position + directions[mask[i]];
                    if (OutOfBounds(nextPosition)) { continue; }

                    nextIndex = Flatten(nextPosition);
                    if (ObstructedNode(nextIndex)) { continue; }

                    cost = Convert.ToUInt16(intField[index] + costField[nextIndex]);
                    if (NotExplored(nextIndex) || LargeCostDiff(cost, nextIndex))
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
        /// Checks if the next node has a lower integration cost and
        /// the difference is greater than the minimum cost difference
        /// </summary>
        /// <param name="cost">The new cost to move to this node</param>
        /// <param name="index">The index for the node in the integration field</param>
        public bool LargeCostDiff(int cost, int index)
        {
            bool lowerCost = cost < intField[index];
            bool largeDiff = intField[index] - cost > minCostDiff;
            return lowerCost && largeDiff;
        }

        /// <summary>
        /// Checks if the node has already been added to the open list
        /// </summary>
        /// <param name="index">The index for the node in the integration field</param>
        public bool NotExplored(int index)
        {
            return intField[index] == ushort.MaxValue;
        }

        /// <summary>
        /// Checks if a node is obstructed in the cost field
        /// </summary>
        /// <param name="index">The index for the node in the cost field</param>
        public bool ObstructedNode(int index)
        {
            return costField[index] == byte.MaxValue;
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