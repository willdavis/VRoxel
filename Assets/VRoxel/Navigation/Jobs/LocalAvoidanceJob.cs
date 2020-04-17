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
            if (minBucket.Equals(maxBucket)) { ResolveCollisions(i, minBucket); }
            else { ResolveCollisions(i, minBucket, maxBucket); }

            // normalize the final direction back into a unit vector
            directions[i] = math.normalizesafe(directions[i], float3.zero);
        }

        /// <summary>
        /// calculates the spatial bucket that contains the given position
        /// </summary>
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
        /// resolves all collisions with an agent for all
        /// spatial buckets from {min} to {max}
        /// </summary>
        public void ResolveCollisions(int i, int3 min, int3 max)
        {
            int3 bucket = int3.zero;
            for (int x = min.x; x < max.x; x++)
            {
                bucket.x = x;
                for (int y = min.y; y < max.y; y++)
                {
                    bucket.y = y;
                    for (int z = min.z; z < max.z; z++)
                    {
                        bucket.z = z;
                        ResolveCollisions(i, bucket);
                    }
                }
            }
        }

        /// <summary>
        /// resolves all collisions with an agent from a single spatial bucket
        /// </summary>
        public void ResolveCollisions(int i, int3 bucket)
        {
            bool hasValues;
            float3 agent = float3.zero;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            int maxDepth = 200;
            float3 distance = float3.zero;

            hasValues = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValues)
            {
                if (count == maxDepth) { break; }
                count++;

                if (!positions[i].Equals(agent))
                {
                    distance = positions[i] - agent;
                    if (AgentCollision(distance))
                        ResolveAgentCollision(i, distance);
                }

                hasValues = spatialMap.TryGetNextValue(out agent, ref iter);
            }
        }

        /// <summary>
        /// test if the agent is colliding with another agent
        /// </summary>
        public bool AgentCollision(float3 distance)
        {
            float length = math.length(distance) - radius;
            if (length > radius) { return false; }
            return true;
        }

        /// <summary>
        /// adjusts the agents desired direction to resolve the collision
        /// </summary>
        public void ResolveAgentCollision(int i, float3 direction)
        {
            directions[i] += math.normalizesafe(direction, float3.zero);
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