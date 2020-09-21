﻿using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct ResolveCollisionBehavior : IJobParallelFor
    {
        public int maxDepth;

        /// <summary>
        /// the size of all spatial buckets
        /// </summary>
        public int3 size;

        /// <summary>
        /// the reference to the voxel world
        /// </summary>
        public AgentWorld world;

        /// <summary>
        /// the collision properties for this archetype
        /// </summary>
        public AgentCollision collision;

        /// <summary>
        /// the current steering forces acting on each agent
        /// </summary>
        public NativeArray<float3> steering;

        /// <summary>
        /// Refrences each agents movement configuration
        /// </summary>
        [ReadOnly] public NativeArray<int> movement;

        /// <summary>
        /// A lookup table for all agent movement configurations
        /// </summary>
        [ReadOnly] public NativeArray<AgentMovement> movementConfigs;

        /// <summary>
        /// the active agents in the scene
        /// </summary>
        [ReadOnly] public NativeArray<bool> active;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        [ReadOnly] public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// the spatial map of all agent positions in the scene
        /// </summary>
        [ReadOnly] public NativeMultiHashMap<int3, SpatialMapData> spatialMap;

        public void Execute(int i)
        {
            if (!active[i]) { return; }

            AgentKinematics agent = agents[i];
            float3 min = agent.position + new float3(-collision.radius, -collision.radius, -collision.radius);
            float3 max = agent.position + new float3( collision.radius,  collision.radius,  collision.radius);

            int3 minBucket = GetSpatialBucket(min);
            int3 maxBucket = GetSpatialBucket(max);

            if (minBucket.Equals(maxBucket))
                ResolveCollisions(i, minBucket);
            else
                ResolveCollisions(i, minBucket, maxBucket);
        }

        public bool Collision(AgentKinematics self, SpatialMapData target)
        {
            if (self.position.Equals(target.position)) { return false; }

            float distance = math.length(self.position - target.position);
            return distance <= collision.radius + target.radius;
        }

        public void ApplyCollisionForce(int i, SpatialMapData target)
        {
            // calculate the distance bewteen the two agents
            float3 direction = agents[i].position - target.position;
            float distance = math.length(direction);
            if (distance == 0) { return; }

            // calculate the mass difference between the two agents
            float mass = movementConfigs[movement[i]].mass;
            float massRatio = target.mass / mass;

            // calcuate the collision force for this agent
            float combinedRadius = collision.radius + target.radius;
            float penetration = combinedRadius / distance;
            float forceScale = penetration * massRatio;

            // apply the collision to the agents steering force
            direction = math.normalizesafe(direction, float3.zero);
            steering[i] += direction * forceScale;
        }

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

        public void ResolveCollisions(int i, int3 bucket)
        {
            bool hasValue;
            SpatialMapData agent;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            hasValue = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValue)
            {
                if (count == maxDepth)
                    break;

                if (Collision(agents[i], agent))
                    ApplyCollisionForce(i, agent);

                count++;
                hasValue = spatialMap
                    .TryGetNextValue(out agent, ref iter);
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

        /// <summary>
        /// Calculates an int3 (Vector3Int) grid coordinate
        /// from a float3 (Vector3) scene position
        /// </summary>
        /// <param name="position">A position in the scene</param>
        public int3 GridPosition(float3 position)
        {
            float3 adjusted = position;
            int3 gridPosition = int3.zero;
            quaternion rotation = math.inverse(world.rotation);

            adjusted += world.offset * -1f;     // adjust for the worlds offset
            adjusted *= 1 / world.scale;        // adjust for the worlds scale
            adjusted = math.rotate(rotation, adjusted);     // adjust for the worlds rotation
            adjusted += world.center;           // adjust for the worlds center

            gridPosition.x = (int)math.floor(adjusted.x);
            gridPosition.y = (int)math.floor(adjusted.y);
            gridPosition.z = (int)math.floor(adjusted.z);

            return gridPosition;
        }
    }
}
