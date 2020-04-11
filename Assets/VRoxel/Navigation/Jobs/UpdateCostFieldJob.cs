using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    /// <summary>
    /// updates the cost field by testing if each voxel
    /// can be walked or climbed on.
    /// </summary>
    [BurstCompile]
    public struct UpdateCostFieldJob : IJobParallelFor
    {
        /// <summary>
        /// the height, in blocks, of the
        /// agents that use this field
        /// </summary>
        public int height;

        /// <summary>
        /// the size of the cost field
        /// </summary>
        public int3 size;

        /// <summary>
        /// a reference to all block types
        /// </summary>
        [ReadOnly]
        public NativeArray<Block> blocks;

        /// <summary>
        /// a reference to all 27 directions
        /// </summary>
        [ReadOnly]
        public NativeArray<int3> directions;

        /// <summary>
        /// the directions to compare as climbable
        /// </summary>
        [ReadOnly]
        public NativeArray<int> directionMask;

        /// <summary>
        /// the block indexes for each voxel in the world.
        /// </summary>
        [ReadOnly]
        public NativeArray<byte> voxels;

        /// <summary>
        /// the movement costs for each block in the world.
        /// Blocks with a value of 255 are obstructed.
        /// </summary>
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
        /// Test for a solid block and N air blocks above,
        /// where N is the agent height
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
        /// Calculate an array index from a int3 (Vector3Int) point
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate a int3 (Vector3Int) point from an array index
        /// </summary>
        public int3 UnFlatten(int index)
        {
            int x = index / (size.y * size.z);
            int y = (index - x * size.y * size.z) / size.z;
            int z = index - x * size.y * size.z - y * size.z;
            return new int3(x,y,z);
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

    /// <summary>
    /// navigation data for blocks in the voxel world
    /// </summary>
    public struct Block
    {
        public bool solid;
        public byte cost;
    }
}