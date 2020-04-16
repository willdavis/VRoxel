using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct BuildSpatialMapJob : IJobParallelForTransform
    {
        /// <summary>
        /// the size for each spatial bucket
        /// </summary>
        public int3 size;

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

        [WriteOnly]
        public NativeArray<float3> positions;

        [WriteOnly]
        public NativeMultiHashMap<int3, float3>.ParallelWriter spatialMap;

        public void Execute(int i, TransformAccess transform)
        {
            int3 grid = GridPosition(transform.position);
            int3 bucket = new int3(
                grid.x / size.x,
                grid.y / size.y,
                grid.z / size.z
            );

            positions[i] = transform.position;
            spatialMap.Add(bucket, transform.position);
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
    }
}