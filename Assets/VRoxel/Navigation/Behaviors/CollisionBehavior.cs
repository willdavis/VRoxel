using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A steering behavior that provides collision resolution
    /// </summary>
    [BurstCompile]
    public struct CollisionBehavior : IJobParallelFor
    {
        /// <summary>
        /// the maximum amount of agents to test when resolving collision
        /// </summary>
        public int maxDepth;

        /// <summary>
        /// the minimum distance required for collision detection
        /// </summary>
        public float minDistance;

        /// <summary>
        /// the minimum collision force required to separate the agents
        /// </summary>
        public float minForce;

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
        /// the navigation behaviors for each agent
        /// </summary>
        [ReadOnly] public NativeArray<AgentBehaviors> behaviors;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        [ReadOnly] public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// Refrences each agents movement configuration
        /// </summary>
        [ReadOnly] public NativeArray<int> movement;

        /// <summary>
        /// A lookup table for all agent movement configurations
        /// </summary>
        [ReadOnly] public NativeArray<AgentMovement> movementConfigs;

        /// <summary>
        /// the spatial map of all agent positions in the scene
        /// </summary>
        [ReadOnly] public NativeMultiHashMap<int3, SpatialMapData> spatialMap;

        public void Execute(int i)
        {
            AgentBehaviors mask = AgentBehaviors.Active;
            if ((behaviors[i] & mask) == 0) { return; }

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

        /// <summary>
        /// Checks if there is a collision between two agents
        /// using their collision radius and height
        /// </summary>
        public bool Collision(AgentKinematics self, SpatialMapData target)
        {
            // skip if the target agent is the same as the source agent
            if (self.position.Equals(target.position)) { return false; }

            // find the distance between the two agents
            // clamp y-axis to test 2D circle intersection

            float3 delta = self.position - target.position;
            float deltaY = delta.y;
            delta.y = 0;

            // use the combined radius of the two agents to
            // test if their collision radius intersect

            float distance = math.length(delta);
            float radius = collision.radius + target.radius;
            bool intersectCircle = distance <= radius;

            // compare the two agent's height and offset for collisions
            // use delta Y to test that the agents are colliding with each other

            bool intersectHeight;
            if (deltaY >= 0) // target is above the agent
                intersectHeight = deltaY - collision.height < 0;
            else // target is below the agent
                intersectHeight = -deltaY - target.height < 0;

            // agents must be intersecting the 2D circle
            // and height to be considered a collision
            return intersectCircle && intersectHeight;
        }

        /// <summary>
        /// Calculates the required force to separate the two agents
        /// </summary>
        public void ApplyCollisionForce(int i, SpatialMapData target)
        {
            // calculate the distance bewteen the two agents
            float3 direction = agents[i].position - target.position;
            direction.y = 0; // clamp the y-axis to create a 2D force

            // exit if the agents are within the minimum distance
            // this helps prevent excessive collision forces
            float distance = math.length(direction);
            if (distance <= minDistance) { return; }

            // calculate the mass difference between the two agents
            float mass = movementConfigs[movement[i]].mass;
            float massRatio = target.mass / mass;

            // calcuate the collision force for this agent and skip
            // if it does not exceed the minForce, creating a deadband
            float combinedRadius = collision.radius + target.radius;
            float penetration = combinedRadius / distance;
            float forceScale = penetration * massRatio;
            if (forceScale <= minForce) { return; }

            // apply the collision to this agents steering force
            direction = math.normalizesafe(direction, float3.zero);
            steering[i] += direction * forceScale;
        }

        /// <summary>
        /// Check for any collisions and apply a collision force
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
        /// Check for any collisions and apply a collision force
        /// </summary>
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

        /// <summary>
        /// Returns the spatial bucket that the position is inside
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
