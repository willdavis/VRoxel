using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    public struct Block
    {
        public bool solid;
        public byte cost;
    }

    [BurstCompile]
    public struct UpdateCostFieldJob : IJobParallelFor
    {
        public int height;
        public int3 size;

        [ReadOnly]
        public NativeArray<Block> blocks;

        [ReadOnly]
        public NativeArray<int3> directions;

        [ReadOnly]
        public NativeArray<int> directionMask;

        [ReadOnly]
        public NativeArray<byte> voxels;

        [WriteOnly]
        public NativeArray<byte> costField;

        public void Execute(int i)
        {
            byte voxel = voxels[i];
            Block block = blocks[voxel];
            int3 position = UnFlatten(i);

            if (Walkable(block, position))
                costField[i] = block.cost;
            else if (Climbable(block, position))
                costField[i] = block.cost;
            else
                costField[i] = 255;
        }

        /// <summary>
        /// Test for a solid block and N air blocks above, where N is the agent height
        /// </summary>
        public bool Walkable(Block block, int3 position)
        {
            if (!block.solid) { return false; }

            byte nextVoxel;
            Block nextBlock;
            int3 next;

            for (int i = 0; i < height; i++)
            {
                next = position + (directions[1] * (i+1));
                if (OutOfBounds(next)) { return false; }

                nextVoxel = voxels[Flatten(next)];
                nextBlock = blocks[nextVoxel];
                if (nextBlock.solid) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Test for solid N,E,S,W neighbors around and air block
        /// to determine if a block is climbable
        /// </summary>
        public bool Climbable(Block block, int3 position)
        {
            if (block.solid) { return false; }

            int3 next;
            byte nextVoxel;
            Block nextBlock;
            bool climbable = false;

            for (int i = 0; i < directionMask.Length; i++)
            {
                next = position + directions[directionMask[i]];
                if (OutOfBounds(next)) { continue; }

                nextVoxel = voxels[Flatten(next)];
                nextBlock = blocks[nextVoxel];
                if (nextBlock.solid) { climbable = true; break; }
            }

            return climbable;
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