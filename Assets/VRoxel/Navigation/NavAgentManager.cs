using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

using VRoxel.Core;
using VRoxel.Core.Data;

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
        /// <summary>
        /// The different archetypes used in the agent configurations
        /// </summary>
        public List<NavAgentArchetype> archetypes;

        /// <summary>
        /// The configurations for each type of agent in the scene
        /// </summary>
        public List<NavAgentConfiguration> configurations;

        public int3 spatialBucketSize;

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
        NativeArray<bool> m_agentActive;


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
        /// A reference to the voxel blocks
        /// </summary>
        protected BlockManager m_blockManager;

        /// <summary>
        /// The background job to move each agent in the scene
        /// </summary>
        protected JobHandle m_movingAgents;


        /// <summary>
        /// The combined handle for updating all flow fields
        /// </summary>
        protected JobHandle m_updatingFlowFields;

        /// <summary>
        /// The handles for updating each archetypes flow field
        /// </summary>
        protected NativeArray<JobHandle> m_updatingHandles;


        protected NativeArray<int3> m_directions;
        protected NativeArray<int> m_directionsNESW;


        protected List<NativeArray<Block>> m_blockTypes;
        protected List<NativeQueue<int3>> m_openNodes;
        protected List<NativeArray<byte>> m_costFields;
        protected List<NativeArray<byte>> m_flowFields;
        protected List<NativeArray<ushort>> m_intFields;


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

            // initialize agent movement configurations
            int configCount = configurations.Count;
            m_movementTypes = new NativeArray<AgentMovement>(configCount, Allocator.Persistent);

            for (int i = 0; i < configCount; i++)
                m_movementTypes[i] = configurations[i].movement;

            // initialize the agent properties
            m_transformAccess = new TransformAccessArray(transforms);
            m_agentActive = new NativeArray<bool>(m_totalAgents, Allocator.Persistent);
            m_agentSteering = new NativeArray<float3>(m_totalAgents, Allocator.Persistent);
            m_agentMovementTypes = new NativeArray<int>(m_totalAgents, Allocator.Persistent);
            m_agentKinematics = new NativeArray<AgentKinematics>(m_totalAgents, Allocator.Persistent);

            // initialize the agent spatial map
            m_spatialMap = new NativeMultiHashMap<int3, float3>(
                m_totalAgents, Allocator.Persistent);
            m_spatialMapWriter = m_spatialMap.AsParallelWriter();

            // initialize the flow field data structures
            int length = world.size.x * world.size.y * world.size.z;
            m_flowFields = new List<NativeArray<byte>>(archetypes.Count);
            m_costFields = new List<NativeArray<byte>>(archetypes.Count);
            m_intFields = new List<NativeArray<ushort>>(archetypes.Count);
            m_openNodes = new List<NativeQueue<int3>>(archetypes.Count);

            // initialize flow fields for each archetype
            for (int i = 0; i < archetypes.Count; i++)
            {
                m_openNodes.Add(new NativeQueue<int3>(Allocator.Persistent));
                m_flowFields.Add(new NativeArray<byte>(length, Allocator.Persistent));
                m_costFields.Add(new NativeArray<byte>(length, Allocator.Persistent));
                m_intFields.Add(new NativeArray<ushort>(length, Allocator.Persistent));
            }

            m_updatingHandles = new NativeArray<JobHandle>(archetypes.Count, Allocator.Persistent);

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
            byte defaultCost = 1;
            int blockCount = m_blockManager.blocks.Count;
            m_blockTypes = new List<NativeArray<Block>>();
            for (int a = 0; a < archetypes.Count; a++)
            {
                NativeArray<Block> blocks = new NativeArray<Block>(
                    blockCount, Allocator.Persistent);

                for (int i = 0; i < blockCount; i++)
                {
                    BlockConfiguration block = m_blockManager.blocks[i];
                    int index = archetypes[a].movementCosts
                        .FindIndex(x=>x.block == block);
                    Block navBlock = new Block();

                    if (index == -1)
                        navBlock.cost = defaultCost;
                    else
                        navBlock.cost = archetypes[a]
                            .movementCosts[index].cost;

                    navBlock.solid = block.collidable;
                    blocks[i] = navBlock;
                }

                m_blockTypes.Add(blocks);
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
        /// Updates the flow fields for each agent archetype to target the new goal position
        /// </summary>
        public JobHandle UpdateFlowFields(Vector3Int goal, JobHandle dependsOn = default)
        {
            int3 target = new int3(goal.x, goal.y, goal.z);

            if (!m_updatingFlowFields.IsCompleted)
                m_updatingFlowFields.Complete();

            for (int i = 0; i < archetypes.Count; i++)
                m_updatingHandles[i] = UpdateFlowField(i, target, dependsOn);

            m_updatingFlowFields = JobHandle.CombineDependencies(m_updatingHandles);
            return m_updatingFlowFields;
        }

        /// <summary>
        /// Disposes any unmanaged memory from the agent manager
        /// </summary>
        public void Dispose()
        {
            // dispose the agent data
            if (m_agentActive.IsCreated){ m_agentActive.Dispose(); }
            if (m_agentSteering.IsCreated) { m_agentSteering.Dispose(); }
            if (m_agentKinematics.IsCreated) { m_agentKinematics.Dispose(); }
            if (m_transformAccess.isCreated) { m_transformAccess.Dispose(); }
            if (m_agentMovementTypes.IsCreated) { m_agentMovementTypes.Dispose(); }

            // dispose the spatial map data
            if (m_spatialMap.IsCreated) { m_spatialMap.Dispose(); }

            // dispose lookup tables
            if (m_directions.IsCreated) { m_directions.Dispose(); }
            if (m_directionsNESW.IsCreated) { m_directionsNESW.Dispose(); }
            if (m_movementTypes.IsCreated) { m_movementTypes.Dispose(); }

            // dispose the flow field data
            if (m_updatingHandles.IsCreated) { m_updatingHandles.Dispose(); }

            if (m_blockTypes != null)
                foreach (var item in m_blockTypes)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_flowFields != null)
                foreach (var item in m_flowFields)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_costFields != null)
                foreach (var item in m_costFields)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_intFields != null)
                foreach (var item in m_intFields)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_openNodes != null)
                foreach (var item in m_openNodes)
                    if (item.IsCreated) { item.Dispose(); }
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            m_blockManager = GetComponent<BlockManager>();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        #endregion
        //-------------------------------------------------

        /// <summary>
        /// Schedules background jobs to update a flow field to target a new goal position
        /// </summary>
        protected JobHandle UpdateFlowField(int index, int3 target, JobHandle dependsOn = default)
        {
            int height = archetypes[index].collision.height;
            int length = m_world.size.x * m_world.size.y * m_world.size.z;
            int3 worldSize = new int3(m_world.size.x, m_world.size.y, m_world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = m_world.data.voxels,
                directionMask = m_directionsNESW,
                directions = m_directions,
                costField = m_costFields[index],
                blocks = m_blockTypes[index],
                size = worldSize,
                height = height
            };
            JobHandle costHandle = costJob.Schedule(length, 1, dependsOn);


            ClearIntFieldJob clearJob = new ClearIntFieldJob()
            {
                intField = m_intFields[index]
            };
            JobHandle clearHandle = clearJob.Schedule(length, 1, costHandle);


            UpdateIntFieldJob intJob = new UpdateIntFieldJob()
            {
                directions = m_directions,
                costField = m_costFields[index],
                intField = m_intFields[index],
                open = m_openNodes[index],
                size = worldSize,
                goal = target
            };
            JobHandle intHandle = intJob.Schedule(clearHandle);


            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob()
            {
                directions = m_directions,
                flowField = m_flowFields[index],
                intField = m_intFields[index],
                size = worldSize,
            };

            return flowJob.Schedule(length, 1, intHandle);
        }

        /// <summary>
        /// Schedules background jobs to move each agent in the scene by the given delta time
        /// </summary>
        protected JobHandle ScheduleAgentMovement(float dt, JobHandle dependsOn = default)
        {
            int3 worldSize = new int3(m_world.size.x, m_world.size.y, m_world.size.z);

            AgentWorld world = new AgentWorld();
            world.offset = m_world.transform.position;
            world.rotation = m_world.transform.rotation;
            world.center = m_world.data.center;
            world.scale = m_world.scale;

            BuildSpatialMapJob spaceJob = new BuildSpatialMapJob()
            {
                world = world,
                active = m_agentActive,
                agents = m_agentKinematics,

                spatialMap = m_spatialMapWriter,
                size = spatialBucketSize
            };
            JobHandle spaceHandle = spaceJob.Schedule(m_transformAccess, dependsOn);

            FlowFieldSeekJob seekJob = new FlowFieldSeekJob()
            {
                world = world,
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes,

                flowField = m_flowFields[0],
                flowDirections = m_directions,
                flowFieldSize = worldSize,

                active = m_agentActive,
                agents = m_agentKinematics,
                steering = m_agentSteering,
            };
            JobHandle seekHandle = seekJob.Schedule(m_totalAgents, 1, spaceHandle);

            AvoidCollisionBehavior avoidJob = new AvoidCollisionBehavior()
            {
                avoidForce = avoidForce,
                avoidRadius = avoidRadius,
                avoidDistance = avoidDistance,
                maxDepth = maxAvoidDepth,

                world = world,
                active = m_agentActive,
                agents = m_agentKinematics,
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

                world = world,
                active = m_agentActive,
                agents = m_agentKinematics,
                steering = m_agentSteering,

                size = spatialBucketSize,
                spatialMap = m_spatialMap
            };
            JobHandle queueHandle = queueJob.Schedule(m_totalAgents, 1, avoidHandle);

            ResolveCollisionBehavior collisionJob = new ResolveCollisionBehavior()
            {
                collisionForce = collisionForce,
                collisionRadius = collisionRadius,
                maxDepth = maxCollisionDepth,

                world = world,
                active = m_agentActive,
                agents = m_agentKinematics,
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

                world = world,
                active = m_agentActive,
                agents = m_agentKinematics,
                steering = m_agentSteering,
                deltaTime = dt,

                flowField = m_flowFields[0],
                flowFieldSize = worldSize,
            };

            m_movingAgents = moveJob.Schedule(m_transformAccess, collisionHandle);
            return m_movingAgents;
        }
    }
}
