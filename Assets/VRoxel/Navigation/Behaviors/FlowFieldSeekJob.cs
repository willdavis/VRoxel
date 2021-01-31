using VRoxel.Navigation.Agents;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct FlowFieldSeekJob : IJobParallelFor
    {
        /// <summary>
        /// the size of the flow field
        /// </summary>
        public int3 flowFieldSize;

        /// <summary>
        /// the reference to the voxel world
        /// </summary>
        public AgentWorld world;

        /// <summary>
        /// the current steering forces acting on each agent
        /// </summary>
        [WriteOnly] public NativeArray<float3> steering;

        /// <summary>
        /// the navigation behaviors for each agent
        /// </summary>
        [ReadOnly] public NativeArray<AgentBehaviors> behaviors;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        [ReadOnly] public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// the direction indexes for each block in the world.
        /// Blocks with a value of 0 have no direction
        /// </summary>
        [ReadOnly] public NativeArray<byte> flowField;

        /// <summary>
        /// a reference to all 27 directions
        /// </summary>
        [ReadOnly] public NativeArray<int3> flowDirections;

        [ReadOnly] public NativeArray<AgentMovement> movementTypes;
        [ReadOnly] public NativeArray<int> agentMovement;

        public void Execute(int i)
        {
            AgentBehaviors mask = AgentBehaviors.Seeking;
            if ((behaviors[i] & mask) == 0) { return; }

            AgentKinematics agent = agents[i];
            int3 grid = GridPosition(agent.position);
            grid += new int3(0, -1, 0);

            if (OutOfBounds(grid))
            {
                steering[i] = float3.zero;
                return;
            }

            int fieldIndex = Flatten(grid);
            byte directionIndex = flowField[fieldIndex];

            if (directionIndex == 0)    // no direction
            {
                steering[i] = float3.zero;
                return;
            }

            int3 flowDirection = flowDirections[directionIndex];
            int3 nextGrid = grid + flowDirection + new int3(0, 1, 0);
            float3 nextPosition = ScenePosition(nextGrid);

            float3 desired = nextPosition - agent.position;
            desired = math.normalizesafe(desired, float3.zero);
            desired *= movementTypes[agentMovement[i]].topSpeed;

            steering[i] = desired - agent.velocity;
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
        /// Calculates the float3 (Vector3) scene position
        /// from an int3 (Vector3Int) grid coordinate
        /// </summary>
        /// <param name="gridPosition">A point in the voxel grid</param>
        public float3 ScenePosition(int3 gridPosition)
        {
            float3 position = gridPosition;
            position += new float3(1,1,1) * 0.5f;   // adjust for the chunks center
            position += world.center * -1f;         // adjust for the worlds center
            position = math.rotate(world.rotation, position);   // adjust for the worlds rotation
            position *= world.scale;                // adjust for the worlds scale
            position += world.offset;               // adjust for the worlds offset
            return position;
        }

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) grid coordinate
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * flowFieldSize.y * flowFieldSize.z)
                + (point.y * flowFieldSize.z)
                + point.z;
        }

        /// <summary>
        /// Test if the grid position is outside the flow field
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public bool OutOfBounds(int3 point)
        {
            if (point.x < 0 || point.x >= flowFieldSize.x) { return true; }
            if (point.y < 0 || point.y >= flowFieldSize.y) { return true; }
            if (point.z < 0 || point.z >= flowFieldSize.z) { return true; }
            return false;
        }
    }
}