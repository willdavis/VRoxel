using VRoxel.Navigation.Agents;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct MoveAgentJob : IJobParallelForTransform
    {
        /// <summary>
        /// the elapsed time since last frame
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// the maximum steering force that can be applied to an agent
        /// </summary>
        public float maxForce;

        /// <summary>
        /// the reference to the voxel world
        /// </summary>
        public AgentWorld world;

        /// <summary>
        /// the size of the flow field
        /// </summary>
        public int3 flowFieldSize;

        /// <summary>
        /// the direction indexes for each block in the world.
        /// Blocks with a value of 0 have no direction
        /// </summary>
        [ReadOnly] public NativeArray<byte> flowField;

        /// <summary>
        /// the active agents in the scene
        /// </summary>
        [ReadOnly] public NativeArray<bool> active;

        /// <summary>
        /// the position and velocity of each agent in the scene
        /// </summary>
        public NativeArray<AgentKinematics> agents;

        /// <summary>
        /// the current steering forces acting on each agent
        /// </summary>
        public NativeArray<float3> steering;

        [ReadOnly] public NativeArray<AgentMovement> movementTypes;
        [ReadOnly] public NativeArray<int> agentMovement;

        public void Execute(int i, TransformAccess transform)
        {
            if (!active[i]) { return; }

            float3 up = new float3(0,1,0);
            float3 position = transform.position;
            quaternion rotation = transform.rotation;

            AgentMovement movement = movementTypes[agentMovement[i]];
            AgentKinematics agent  = agents[i];

            steering[i] = Clamp(steering[i], maxForce);
            steering[i] = steering[i] / movement.mass;

            agent.velocity = Clamp(agent.velocity + steering[i], movement.topSpeed);
            steering[i] = float3.zero;  // reset steering forces for next frame
            agents[i] = agent;  // update the kinematics for the next frame

            float3 nextPosition = agent.position + agent.velocity * deltaTime;
            int3 nextGrid = GridPosition(nextPosition);

            if (!OutOfBounds(nextGrid) && !Obstructed(nextGrid))
                transform.position = nextPosition;

            if (agent.velocity.Equals(float3.zero)) { return; }
            quaternion look = quaternion.LookRotation(agent.velocity, up);
            transform.rotation = math.slerp(rotation, look, movement.turnSpeed * deltaTime);
        }

        /// <summary>
        /// limits the magnitude of a vector to the given max length
        /// </summary>
        public float3 Clamp(float3 vector, float max)
        {
            float length = max / math.length(vector);
            if (length < 1f) { return vector * length; }
            else { return vector; }
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

        public bool Obstructed(int3 point)
        {
            int index = Flatten(point);
            byte direction = flowField[index];

            if (direction == 0) { return true; }
            return false;
        }
    }
}