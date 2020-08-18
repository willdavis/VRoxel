using VRoxel.Navigation.Agents;
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
        /// the reference to the voxel world
        /// </summary>
        public AgentWorld world;

        /// <summary>
        /// the active agents in the scene
        /// </summary>
        [ReadOnly] public NativeArray<bool> active;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// the spatial map of all agent positions in the scene
        /// </summary>
        [WriteOnly] public NativeMultiHashMap<int3, float3>.ParallelWriter spatialMap;

        public void Execute(int i, TransformAccess transform)
        {
            if (!active[i]) { return; }

            int3 grid = GridPosition(transform.position);
            int3 bucket = new int3(
                grid.x / size.x,
                grid.y / size.y,
                grid.z / size.z
            );

            Agents.AgentKinematics agent = agents[i];
            agent.position = transform.position;
            agents[i] = agent;

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