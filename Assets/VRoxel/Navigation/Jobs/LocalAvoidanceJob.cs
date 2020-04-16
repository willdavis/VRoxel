using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct LocalAvoidanceJob : IJobParallelFor
    {
        /// <summary>
        /// the collision radius for all agents
        /// </summary>
        public float radius;

        /// <summary>
        /// the size of all spatial buckets
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

        /// <summary>
        /// the desired direction for each agent
        /// </summary>
        [WriteOnly]
        public NativeArray<float3> directions;

        /// <summary>
        /// the current position of each agent
        /// </summary>
        [ReadOnly]
        public NativeArray<float3> positions;

        /// <summary>
        /// the position of each agent mapped to subsections of the world
        /// </summary>
        [ReadOnly]
        public NativeMultiHashMap<int3, float3> spatialMap;

        public void Execute(int i)
        {
            // get the spatial subdivision containing the current agent
            int3 bucket = GetSpatialBucket(positions[i]);

            // get any adjacent buckets that overlap with the agents radius
            // query each bucket for other agents that are colliding with the agent
                // calculate inverse direction of collision
                // add resolution vector to the desired direction
                // exit bucket if maxDepth is reached
            // normalize the new desired direction vector
        }

        public int3 GetSpatialBucket(float3 position)
        {
            int3 grid = GridPosition(position);
            int3 bucket = new int3(
                grid.x / size.x,
                grid.y / size.y,
                grid.z / size.z
            );
            return bucket;
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