using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    public struct FlowDirectionJob : IJobParallelForTransform
    {
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
            Vector3Int gridPosition = GridPosition(transform.position);
            Vector3Int nextGridPosition = gridPosition + Vector3Int.up;
            Vector3 nextPosition = ScenePosition(nextGridPosition);
            Vector3 direction = (nextPosition - transform.position).normalized;
            directions[i] = direction;
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
    }
}