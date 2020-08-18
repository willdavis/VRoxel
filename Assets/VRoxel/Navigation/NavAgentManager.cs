using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;
using VRoxel.Core;

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A component to help manage navigation agents in the scene
    /// </summary>
    public class NavAgentManager : MonoBehaviour
    {
        public int3 spatialBucketSize;

        // agent settings
        public int height;

        // moving
        public float maxForce;

        // queuing
        public float brakeForce;
        public float queueRadius;
        public float queueDistance;
        public int maxQueueDepth;

        // avoidance
        public float avoidForce;
        public float avoidRadius;
        public float avoidDistance;
        public int maxAvoidDepth;

        // collision
        public float collisionForce;
        public float collisionRadius;
        public int maxCollisionDepth;
        public NativeArray<bool> activeAgents { get { return m_agentActive; } }
        NativeArray<float3> m_agentPositions;
        NativeArray<float3> m_agentVelocity;
        NativeArray<bool> m_agentActive;



        /// <summary>
        /// The configurations for each type of agent that will be managed
        /// </summary>
        public List<NavAgentConfiguration> configurations;

        /// <summary>
        /// The different block types used for pathfinding
        /// </summary>
        public List<VRoxel.Core.Block> blockTypes;



        /// <summary>
        /// The current kinematic properties for each agent
        /// </summary>
        protected NativeArray<AgentKinematics> m_agentKinematics;

        /// <summary>
        /// The current steering forces acting on each agent
        /// </summary>
        protected NativeArray<float3> m_agentSteering;

        /// <summary>
        /// The async transform access to each agent's transform
        /// </summary>
        protected TransformAccessArray m_transformAccess;

        /// <summary>
        /// Contains indexes to the movement configuration of each agent
        /// </summary>
        protected NativeArray<int> m_agentMovementTypes;

        /// <summary>
        /// A reference to the different agent movement configurations
        /// </summary>
        protected NativeArray<AgentMovement> m_movementTypes;

        /// <summary>
        /// Caches the total number of agents being managed
        /// </summary>
        protected int m_totalAgents;

        /// <summary>
        /// A reference to the voxel world
        /// </summary>
        protected World m_world;

        /// <summary>
        /// The background job to move each agent in the scene
        /// </summary>
        protected JobHandle m_movingAgents;

        /// <summary>
        /// The background job to update each of the flow fields
        /// </summary>
        protected JobHandle m_updatingPathfinding;


        protected NativeQueue<int3> m_openNodes;
        protected NativeArray<byte> m_flowField;
        protected NativeArray<byte> m_costField;
        protected NativeArray<ushort> m_intField;

        protected NativeArray<int3> m_directions;
        protected NativeArray<int> m_directionsNESW;
        protected NativeArray<Block> m_blockTypes;

        protected NativeMultiHashMap<int3, float3> m_spatialMap;
        protected NativeMultiHashMap<int3, float3>.ParallelWriter m_spatialMapWriter;

        //-------------------------------------------------
        #region Public API

        /// <summary>
        /// Initializes the agent manager with an array of agent transforms
        /// </summary>
        public virtual void Initialize(World world, Transform[] transforms)
        {
            Dispose();  // clear any existing memory
            m_totalAgents = transforms.Length;
            m_world = world;

            // old agent properties
            m_agentPositions = new NativeArray<float3>(m_totalAgents, Allocator.Persistent);
            m_agentVelocity = new NativeArray<float3>(m_totalAgents, Allocator.Persistent);
            m_agentActive = new NativeArray<bool>(m_totalAgents, Allocator.Persistent);

            // initialize agent movement configurations
            int configCount = configurations.Count;
            m_movementTypes = new NativeArray<AgentMovement>(configCount, Allocator.Persistent);

            for (int i = 0; i < configCount; i++)
                m_movementTypes[i] = configurations[i].movement;

            // initialize the agent properties
            m_transformAccess = new TransformAccessArray(transforms);
            m_agentSteering = new NativeArray<float3>(m_totalAgents, Allocator.Persistent);
            m_agentMovementTypes = new NativeArray<int>(m_totalAgents, Allocator.Persistent);
            m_agentKinematics = new NativeArray<AgentKinematics>(m_totalAgents, Allocator.Persistent);

            // initialize the agent spatial map
            m_spatialMap = new NativeMultiHashMap<int3, float3>(
                m_totalAgents, Allocator.Persistent);
            m_spatialMapWriter = m_spatialMap.AsParallelWriter();

            // initialize the flow field data structures
            int length = world.size.x * world.size.y * world.size.z;
            m_openNodes = new NativeQueue<int3>(Allocator.Persistent);
            m_flowField = new NativeArray<byte>(length, Allocator.Persistent);
            m_costField = new NativeArray<byte>(length, Allocator.Persistent);
            m_intField  = new NativeArray<ushort>(length, Allocator.Persistent);

            // cache all directions
            m_directions = new NativeArray<int3>(27, Allocator.Persistent);
            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Core.Direction3Int.Directions[i];
                m_directions[i] = new int3(dir.x, dir.y, dir.z);
            }

            // cache directions for climbable check
            m_directionsNESW = new NativeArray<int>(4, Allocator.Persistent);
            m_directionsNESW[0] = 3;
            m_directionsNESW[1] = 5;
            m_directionsNESW[2] = 7;
            m_directionsNESW[3] = 9;

            // convert blocks to a struct and cache the data
            int blockCount = blockTypes.Count;
            m_blockTypes = new NativeArray<Block>(blockCount, Allocator.Persistent);
            for (int i = 0; i < blockCount; i++)
            {
                Core.Block block = blockTypes[i];
                Block navBlock = new Block();
                navBlock.solid = block.isSolid;

                if (navBlock.solid) { navBlock.cost = 1; }
                else { navBlock.cost = 2; }

                m_blockTypes[i] = navBlock;
            }

            // configure each agents movement type
            for (int i = 0; i < m_totalAgents; i++)
            {
                NavAgent agent = transforms[i].GetComponent<NavAgent>();
                int index = configurations.IndexOf(agent.configuration);
                m_agentMovementTypes[i] = index;
            }
        }

        /// <summary>
        /// Schedules background jobs to move all agents using the given delta time
        /// </summary>
        public JobHandle MoveAgents(float dt, JobHandle dependsOn = default)
        {
            m_spatialMap.Clear();

            if (!m_movingAgents.IsCompleted)
                m_movingAgents.Complete();

            return ScheduleAgentMovement(dt, dependsOn);
        }

        /// <summary>
        /// Schedules background jobs to update all flow fields to target the new goal position
        /// </summary>
        public JobHandle UpdatePathfinding(Vector3Int goal, JobHandle dependsOn = default)
        {
            if (!m_updatingPathfinding.IsCompleted)
                m_updatingPathfinding.Complete();

            int3 target = new int3(goal.x, goal.y, goal.z);
            return SchedulePathfindingUpdate(target, dependsOn);
        }

        /// <summary>
        /// Disposes any unmanaged memory from the agent manager
        /// </summary>
        public void Dispose()
        {
            // dispose old agent data
            if (m_agentPositions.IsCreated){ m_agentPositions.Dispose(); }
            if (m_agentVelocity.IsCreated){ m_agentVelocity.Dispose(); }
            if (m_agentActive.IsCreated){ m_agentActive.Dispose(); }

            // dispose the agent data
            if (m_agentSteering.IsCreated) { m_agentSteering.Dispose(); }
            if (m_agentKinematics.IsCreated) { m_agentKinematics.Dispose(); }
            if (m_transformAccess.isCreated) { m_transformAccess.Dispose(); }

            if (m_agentMovementTypes.IsCreated) { m_agentMovementTypes.Dispose(); }
            if (m_movementTypes.IsCreated) { m_movementTypes.Dispose(); }

            // dispose the spatial map data
            if (m_spatialMap.IsCreated) { m_spatialMap.Dispose(); }

            // dispose the flow field data
            if (m_openNodes.IsCreated) { m_openNodes.Dispose(); }
            if (m_costField.IsCreated) { m_costField.Dispose(); }
            if (m_flowField.IsCreated) { m_flowField.Dispose(); }
            if (m_intField.IsCreated)  { m_intField.Dispose();  }

            // dispose lookup tables
            if (m_blockTypes.IsCreated) { m_blockTypes.Dispose(); }
            if (m_directions.IsCreated) { m_directions.Dispose(); }
            if (m_directionsNESW.IsCreated) { m_directionsNESW.Dispose(); }
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        #endregion
        //-------------------------------------------------


        /// <summary>
        /// Schedules background jobs to update all flow fields to target a new destination
        /// </summary>
        protected JobHandle SchedulePathfindingUpdate(int3 target, JobHandle dependsOn = default)
        {
            int length = m_world.size.x * m_world.size.y * m_world.size.z;
            int3 worldSize = new int3(m_world.size.x, m_world.size.y, m_world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = m_world.data.voxels,
                directionMask = m_directionsNESW,
                directions = m_directions,
                costField = m_costField,
                blocks = m_blockTypes,
                size = worldSize,
                height = height
            };
            JobHandle costHandle = costJob.Schedule(length, 1, dependsOn);


            ClearIntFieldJob clearJob = new ClearIntFieldJob()
            {
                intField = m_intField
            };
            JobHandle clearHandle = clearJob.Schedule(length, 1, costHandle);


            UpdateIntFieldJob intJob = new UpdateIntFieldJob()
            {
                directions = m_directions,
                costField = m_costField,
                intField = m_intField,
                open = m_openNodes,
                size = worldSize,
                goal = target
            };
            JobHandle intHandle = intJob.Schedule(clearHandle);


            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob()
            {
                directions = m_directions,
                flowField = m_flowField,
                intField = m_intField,
                size = worldSize,
            };

            m_updatingPathfinding = flowJob.Schedule(length, 1, intHandle);
            return m_updatingPathfinding;
        }

        /// <summary>
        /// Schedules background jobs to move each agent in the scene by the given delta time
        /// </summary>
        protected JobHandle ScheduleAgentMovement(float dt, JobHandle dependsOn = default)
        {
            int3 worldSize = new int3(m_world.size.x, m_world.size.y, m_world.size.z);

            BuildSpatialMapJob spaceJob = new BuildSpatialMapJob()
            {
                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                active = m_agentActive,
                spatialMap = m_spatialMapWriter,
                positions = m_agentPositions,
                size = spatialBucketSize
            };
            JobHandle spaceHandle = spaceJob.Schedule(m_transformAccess, dependsOn);

            FlowFieldSeekJob seekJob = new FlowFieldSeekJob()
            {
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes,

                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                flowField = m_flowField,
                flowDirections = m_directions,
                flowFieldSize = worldSize,

                active = m_agentActive,
                positions = m_agentPositions,
                steering = m_agentSteering,
                velocity = m_agentVelocity,
            };
            JobHandle seekHandle = seekJob.Schedule(m_totalAgents, 1, spaceHandle);

            AvoidCollisionBehavior avoidJob = new AvoidCollisionBehavior()
            {
                avoidForce = avoidForce,
                avoidRadius = avoidRadius,
                avoidDistance = avoidDistance,
                maxDepth = maxAvoidDepth,

                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                active = m_agentActive,
                position = m_agentPositions,
                velocity = m_agentVelocity,
                steering = m_agentSteering,

                spatialMap = m_spatialMap,
                size = spatialBucketSize
            };
            JobHandle avoidHandle = avoidJob.Schedule(m_totalAgents, 1, seekHandle);

            QueueBehavior queueJob = new QueueBehavior()
            {
                maxDepth = maxQueueDepth,
                maxBrakeForce = brakeForce,
                maxQueueRadius = queueRadius,
                maxQueueAhead = queueDistance,

                active = m_agentActive,
                steering = m_agentSteering,
                position = m_agentPositions,
                velocity = m_agentVelocity,

                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                size = spatialBucketSize,
                spatialMap = m_spatialMap
            };
            JobHandle queueHandle = queueJob.Schedule(m_totalAgents, 1, avoidHandle);

            ResolveCollisionBehavior collisionJob = new ResolveCollisionBehavior()
            {
                collisionForce = collisionForce,
                collisionRadius = collisionRadius,
                maxDepth = maxCollisionDepth,

                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                active = m_agentActive,
                position = m_agentPositions,
                velocity = m_agentVelocity,
                steering = m_agentSteering,

                spatialMap = m_spatialMap,
                size = spatialBucketSize
            };
            JobHandle collisionHandle = collisionJob.Schedule(m_totalAgents, 1, queueHandle);

            MoveAgentJob moveJob = new MoveAgentJob()
            {

                maxForce = maxForce,
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes,

                active = m_agentActive,
                steering = m_agentSteering,
                velocity = m_agentVelocity,
                deltaTime = dt,

                world_scale = m_world.scale,
                world_center = m_world.data.center,
                world_offset = m_world.transform.position,
                world_rotation = m_world.transform.rotation,

                flowField = m_flowField,
                flowFieldSize = worldSize,
            };

            m_movingAgents = moveJob.Schedule(m_transformAccess, collisionHandle);
            return m_movingAgents;
        }
    }
}
