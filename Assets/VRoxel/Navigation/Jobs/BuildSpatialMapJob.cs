using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Burst;
using System;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Builds a spatial map of all agent positions
    /// </summary>
    [BurstCompile]
    public struct BuildSpatialMapJob : IJobParallelForTransform
    {
        /// <summary>
        /// the size for each spatial bucket
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
        /// Refrences each agents movement configuration
        /// </summary>
        [ReadOnly] public NativeArray<int> movement;

        /// <summary>
        /// A lookup table for all agent movement configurations
        /// </summary>
        [ReadOnly] public NativeArray<AgentMovement> movementConfigs;

        /// <summary>
        /// the navigation behaviors for each agent
        /// </summary>
        [ReadOnly] public NativeArray<AgentBehaviors> behaviors;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// the spatial map of all agent positions in the scene
        /// </summary>
        [WriteOnly] public NativeMultiHashMap<int3, SpatialMapData>.ParallelWriter spatialMap;

        public void Execute(int i, TransformAccess transform)
        {
            AgentBehaviors mask = AgentBehaviors.Active;
            if ((behaviors[i] & mask) == 0) { return; }

            float mass = movementConfigs[movement[i]].mass;
            int3 grid = GridPosition(transform.position);
            int3 bucket = new int3(
                grid.x / size.x,
                grid.y / size.y,
                grid.z / size.z
            );

            Agents.AgentKinematics agent = agents[i];
            agent.position = transform.position;
            agents[i] = agent;

            SpatialMapData data = new SpatialMapData();
            data.position = transform.position;
            data.height = collision.height;
            data.radius = collision.radius;
            data.mass = mass;

            spatialMap.Add(bucket, data);
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

    /// <summary>
    /// Data container for each agent in the spatial map
    /// </summary>
    [Serializable]
    public struct SpatialMapData
    {
        /// <summary>
        /// The mass of the agent
        /// </summary>
        public float mass;

        /// <summary>
        /// The voxel height of the agent
        /// </summary>
        public int height;

        /// <summary>
        /// The collision radius of the agent
        /// </summary>
        public float radius;

        /// <summary>
        /// The current scene position of the agent
        /// </summary>
        public float3 position;
    }
}