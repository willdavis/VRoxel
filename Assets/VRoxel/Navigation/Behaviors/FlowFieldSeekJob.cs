using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct FlowFieldSeekJob : IJobParallelFor
    {
        /// <summary>
        /// the size of the flow field
        /// </summary>
        public int3 flowFieldSize;

        /// <summary>
        /// the scale of the voxel world
        /// </summary>
        public float world_scale;

        /// <summary>
        /// the scene offset of the world
        /// </summary>
        public float3 world_offset;

        /// <summary>
        /// the center point of the world
        /// </summary>
        public float3 world_center;

        /// <summary>
        /// the orientation of the world
        /// </summary>
        public quaternion world_rotation;

        /// <summary>
        /// the desired direction for each agent
        /// </summary>
        public NativeArray<float3> directions;

        /// <summary>
        /// the current position of each agent
        /// </summary>
        [ReadOnly]
        public NativeArray<float3> positions;

        /// <summary>
        /// the direction indexes for each block in the world.
        /// Blocks with a value of 0 have no direction
        /// </summary>
        [ReadOnly]
        public NativeArray<byte> flowField;

        /// <summary>
        /// a reference to all 27 directions
        /// </summary>
        [ReadOnly]
        public NativeArray<int3> flowDirections;

        public void Execute(int i)
        {
            // exit if the agent already has a direction to move
            //if (!directions[i].Equals(float3.zero)) { return; }

            int3 position = GridPosition(positions[i]);
            position += new int3(0, -1, 0);

            if (OutOfBounds(position))
            {
                directions[i] = float3.zero;
                return;
            }

            int fieldIndex = Flatten(position);
            byte directionIndex = flowField[fieldIndex];

            if (directionIndex == 0)    // no direction
            {
                directions[i] = float3.zero;
                return;
            }

            int3 flowUnitDirection = flowDirections[directionIndex];
            int3 desiredPosition = position + flowUnitDirection + new int3(0, 1, 0);
            float3 desiredScenePosition = ScenePosition(desiredPosition);
            float3 dir = desiredScenePosition - positions[i];

            directions[i] = math.normalizesafe(dir, float3.zero);
        }

        /// <summary>
        /// Calculates an int3 (Vector3Int) grid coordinate
        /// from a float3 (Vector3) scene position
        /// </summary>
        /// <param name="position">A position in the scene</param>
        public int3 GridPosition(float3 position)
        {
            float3 adjusted = position;
            int3 gridPosition = int3.zero;
            quaternion rotation = math.inverse(world_rotation);

            adjusted += world_offset * -1f;     // adjust for the worlds offset
            adjusted *= 1 / world_scale;        // adjust for the worlds scale
            adjusted = math.rotate(rotation, adjusted);     // adjust for the worlds rotation
            adjusted += world_center;           // adjust for the worlds center

            gridPosition.x = (int)math.floor(adjusted.x);
            gridPosition.y = (int)math.floor(adjusted.y);
            gridPosition.z = (int)math.floor(adjusted.z);

            return gridPosition;
        }

        /// <summary>
        /// Calculates the float3 (Vector3) scene position
        /// from an int3 (Vector3Int) grid coordinate
        /// </summary>
        /// <param name="gridPosition">A point in the voxel grid</param>
        public float3 ScenePosition(int3 gridPosition)
        {
            float3 position = gridPosition;
            position += new float3(1,1,1) * 0.5f;   // adjust for the chunks center
            position += world_center * -1f;         // adjust for the worlds center
            position = math.rotate(world_rotation, position);   // adjust for the worlds rotation
            position *= world_scale;                // adjust for the worlds scale
            position += world_offset;               // adjust for the worlds offset
            return position;
        }

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) grid coordinate
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * flowFieldSize.y * flowFieldSize.z)
                + (point.y * flowFieldSize.z)
                + point.z;
        }

        /// <summary>
        /// Test if the grid position is outside the flow field
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public bool OutOfBounds(int3 point)
        {
            if (point.x < 0 || point.x >= flowFieldSize.x) { return true; }
            if (point.y < 0 || point.y >= flowFieldSize.y) { return true; }
            if (point.z < 0 || point.z >= flowFieldSize.z) { return true; }
            return false;
        }
    }
}