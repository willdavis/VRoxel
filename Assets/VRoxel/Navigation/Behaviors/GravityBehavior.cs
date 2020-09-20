using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct GravityBehavior : IJobParallelFor
    {
        /// <summary>
        /// the force of gravity
        /// </summary>
        public float3 gravity;

        /// <summary>
        /// the reference to the voxel world
        /// </summary>
        public AgentWorld world;

        /// <summary>
        /// the current steering forces acting on each agent
        /// </summary>
        public NativeArray<float3> steering;

        /// <summary>
        /// the active agents in the scene
        /// </summary>
        [ReadOnly] public NativeArray<bool> active;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        [ReadOnly] public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// a reference to all block types
        /// </summary>
        [ReadOnly] public NativeArray<Block> blocks;

        /// <summary>
        /// the block indexes for each voxel in the world.
        /// </summary>
        [ReadOnly] public NativeArray<byte> voxels;

        public void Execute(int i)
        {
            if (!active[i]) { return; }

            AgentKinematics agent = agents[i];
            int3 grid = GridPosition(agent.position);
            grid += new int3(0, -1, 0);

            byte voxel = voxels[Flatten(grid)];
            if (OverAir(voxel)) { steering[i] += gravity; }
            if (InSolid(voxel)) { steering[i] += gravity * -0.5f; }
        }

        /// <summary>
        /// Checks if the agent is not on top of a solid block
        /// </summary>
        public bool OverAir(byte voxel)
        {
            return !blocks[voxel].solid;
        }

        /// <summary>
        /// Checks if the agent is inside a solid block
        /// </summary>
        public bool InSolid(byte voxel)
        {
            return blocks[voxel].solid;
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

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) grid coordinate
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * world.size.y * world.size.z)
                + (point.y * world.size.z)
                + point.z;
        }
    }
}