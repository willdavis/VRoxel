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
            // get the spatial buckets that overlap with the agents radius
            int3 minBucket = GetSpatialBucket(positions[i] + new float3(-radius, -radius, -radius));
            int3 maxBucket = GetSpatialBucket(positions[i] + new float3( radius,  radius,  radius));

            // get any adjacent buckets that overlap with the agents radius
            // and check those buckets for collisions with other agents
            if (minBucket.Equals(maxBucket))    // only one bucket to check for collisions
            {
                ResolveCollisions(i, minBucket);
            }
            else    // loop from min to max and check each bucket for collisions
            {
                int3 bucket = int3.zero;
                for (int x = minBucket.x; x < maxBucket.x; x++)
                {
                    bucket.x = x;
                    for (int y = minBucket.y; y < maxBucket.y; y++)
                    {
                        bucket.y = y;
                        for (int z = minBucket.z; z < maxBucket.z; z++)
                        {
                            bucket.z = z;
                            ResolveCollisions(i, bucket);
                        }
                    }
                }
            }
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

        public void ResolveCollisions(int i, int3 bucket)
        {
            bool hasValues;
            float3 agent = float3.zero;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            int maxDepth = 200;
            float3 direction = float3.zero;

            hasValues = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValues)
            {
                if (count == maxDepth) { break; }
                count++;

                if (!positions[i].Equals(agent))
                {
                    direction = positions[i] - agent;
                    if (math.length(direction) <= radius)
                        ResolveAgentCollision(i, direction);
                }

                hasValues = spatialMap.TryGetNextValue(out agent, ref iter);
            }
        }

        public void ResolveAgentCollision(int i, float3 direction)
        {
            float weight = 1 / math.length(direction);
            directions[i] += direction * weight;
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