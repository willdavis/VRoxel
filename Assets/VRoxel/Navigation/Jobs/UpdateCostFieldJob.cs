using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct Block
    {
        public bool solid;
        public byte cost;
    }

    public struct UpdateCostFieldJob : IJobParallelFor
    {
        public int height;
        public Vector3Int size;

        [ReadOnly]
        public NativeArray<Block> blocks;

        [ReadOnly]
        public NativeArray<Vector3Int> directions;

        [ReadOnly]
        public NativeArray<byte> voxels;

        [WriteOnly]
        public NativeArray<byte> costField;

        public void Execute(int i)
        {
            byte voxel = voxels[i];
            Block block = blocks[voxel];
            Vector3Int position = UnFlatten(i);

            if (Walkable(block, position))
                costField[i] = block.cost;
            else
                costField[i] = 255;
        }

        public bool Walkable(Block block, Vector3Int position)
        {
            if (!block.solid) { return false; }

            // check blocks above the current position
            for (int i = 0; i < height; i++)
            {
                Vector3Int next = position + (directions[1] * (i+1));
                if (OutOfBounds(next)) { return false; }

                byte nextVoxel = voxels[Flatten(next)];
                Block nextBlock = blocks[nextVoxel];
                if (nextBlock.solid) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Calculate an array index from a Vector3Int point
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(Vector3Int point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate a Vector3Int point from an array index
        /// </summary>
        public Vector3Int UnFlatten(int index)
        {
            int x = index / (size.y * size.z);
            int y = (index - x * size.y * size.z) / size.z;
            int z = index - x * size.y * size.z - y * size.z;
            return new Vector3Int(x,y,z);
        }

        /// <summary>
        /// Test if a point is inside the flow field
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public bool OutOfBounds(Vector3Int point)
        {
            if (point.x < 0 || point.x >= size.x) { return true; }
            if (point.y < 0 || point.y >= size.y) { return true; }
            if (point.z < 0 || point.z >= size.z) { return true; }
            return false;
        }
    }
}