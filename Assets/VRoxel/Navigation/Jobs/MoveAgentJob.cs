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
        /// the max speed of all agents
        /// </summary>
        public float maxSpeed;

        /// <summary>
        /// the turning speed of all agents
        /// </summary>
        public float turnSpeed;

        /// <summary>
        /// the mass of all agents
        /// </summary>
        public float mass;


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
        /// the size of the flow field
        /// </summary>
        public int3 flowFieldSize;

        /// <summary>
        /// the direction indexes for each block in the world.
        /// Blocks with a value of 0 have no direction
        /// </summary>
        [ReadOnly]
        public NativeArray<byte> flowField;


        /// <summary>
        /// the current status of each agent
        /// </summary>
        [ReadOnly]
        public NativeArray<bool> active;

        /// <summary>
        /// the current steering forces applied to each agent
        /// </summary>
        public NativeArray<float3> steering;

        /// <summary>
        /// the current velocity of each agent
        /// </summary>
        public NativeArray<float3> velocity;


        public void Execute(int i, TransformAccess transform)
        {
            if (!active[i]) { return; }

            float3 up = new float3(0,1,0);
            float3 position = transform.position;
            quaternion rotation = transform.rotation;

            steering[i] = Clamp(steering[i], maxForce);
            steering[i] = steering[i] / mass;

            velocity[i] = Clamp(velocity[i] + steering[i], maxSpeed);
            float3 nextPosition = position + velocity[i] * deltaTime;
            int3 nextGrid = GridPosition(nextPosition);

            if (!OutOfBounds(nextGrid) && !Obstructed(nextGrid))
                transform.position = nextPosition;

            steering[i] = float3.zero;

            if (velocity[i].Equals(float3.zero)) { return; }
            quaternion look = quaternion.LookRotation(velocity[i], up);
            transform.rotation = math.slerp(rotation, look, turnSpeed * deltaTime);
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