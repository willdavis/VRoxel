using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct FlowDirectionJob : IJobParallelForTransform
    {
        public Vector3Int flowFieldSize;

        public float world_scale;
        public Vector3 world_offset;
        public Vector3 world_center;
        public Quaternion world_rotation;

        [WriteOnly]
        public NativeArray<Vector3> directions;

        [ReadOnly]
        public NativeArray<byte> flowField;

        [ReadOnly]
        public NativeArray<Vector3Int> flowDirections;

        public void Execute(int i, TransformAccess transform)
        {
            Vector3Int position = GridPosition(transform.position);

            if (OutOfBounds(position))
            {
                directions[i] = Vector3.zero;
                return;
            }

            int fieldIndex = Flatten(position);
            byte directionIndex = flowField[fieldIndex];
            Vector3Int flowDirection = flowDirections[directionIndex];

            directions[i] = flowDirection;
        }

        /// <summary>
        /// Calculates the voxel grid coordinates for a Vector3 position
        /// </summary>
        /// <param name="position">A position in the scene</param>
        public Vector3Int GridPosition(Vector3 position)
        {
            Vector3 adjusted = position;
            Vector3Int gridPosition = Vector3Int.zero;
            Quaternion rotation = Quaternion.Inverse(world_rotation);

            adjusted += world_offset * -1f;     // adjust for the worlds offset
            adjusted *= 1 / world_scale;        // adjust for the worlds scale
            adjusted = rotation * adjusted;     // adjust for the worlds rotation
            adjusted += world_center;           // adjust for the worlds center

            gridPosition.x = Mathf.FloorToInt(adjusted.x);
            gridPosition.y = Mathf.FloorToInt(adjusted.y);
            gridPosition.z = Mathf.FloorToInt(adjusted.z);

            return gridPosition;
        }

        /// <summary>
        /// Calculates the scene position for a point in the voxel grid
        /// </summary>
        /// <param name="gridPosition">A point in the voxel grid</param>
        public Vector3 ScenePosition(Vector3Int gridPosition)
        {
            Vector3 position = gridPosition;
            position += Vector3.one * 0.5f;         // adjust for the chunks center
            position += world_center * -1f;         // adjust for the worlds center
            position = world_rotation * position;   // adjust for the worlds rotation
            position *= world_scale;                // adjust for the worlds scale
            position += world_offset;               // adjust for the worlds offset
            return position;
        }

        /// <summary>
        /// Calculate a 1D array index from a Vector3Int position
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public int Flatten(Vector3Int point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * flowFieldSize.y * flowFieldSize.z)
                + (point.y * flowFieldSize.z)
                + point.z;
        }

        /// <summary>
        /// Test if the grid position is inside the flow field
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public bool OutOfBounds(Vector3Int point)
        {
            if (point.x < 0 || point.x >= flowFieldSize.x) { return true; }
            if (point.y < 0 || point.y >= flowFieldSize.y) { return true; }
            if (point.z < 0 || point.z >= flowFieldSize.z) { return true; }
            return false;
        }
    }
}