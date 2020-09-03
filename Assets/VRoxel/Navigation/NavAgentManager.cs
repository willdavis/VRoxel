using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

using System.Linq;

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

        /// <summary>
        /// A reference to the voxel world
        /// </summary>
        public World world;

        /// <summary>
        /// A reference to the voxel blocks
        /// </summary>
        public BlockManager blockManager;

        /// <summary>
        /// The size of the spatial buckets used to parition the map
        /// </summary>
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

        public List<NativeArray<bool>> activeAgents { get { return m_agentActive; } }

        /// <summary>
        /// The current active agents of each archetype
        /// </summary>
        protected List<NativeArray<bool>> m_agentActive;

        /// <summary>
        /// The current kinematic properties for each agent
        /// </summary>
        protected List<NativeArray<AgentKinematics>> m_agentKinematics;

        /// <summary>
        /// The current steering forces acting on each agent
        /// </summary>
        protected List<NativeArray<float3>> m_agentSteering;

        /// <summary>
        /// The async transform access to each agent's transform
        /// </summary>
        protected List<TransformAccessArray> m_transformAccess;

        /// <summary>
        /// Contains indexes to the movement configuration of each agent
        /// </summary>
        protected List<NativeArray<int>> m_agentMovementTypes;

        /// <summary>
        /// A reference to the different agent movement configurations
        /// </summary>
        protected NativeArray<AgentMovement> m_movementTypes;

        /// <summary>
        /// Caches the total number of agents per archetype
        /// </summary>
        protected List<int> m_totalAgents;

        /// <summary>
        /// The background job to move all agents in the scene
        /// </summary>
        public JobHandle movingAllAgents { get; private set; }

        /// <summary>
        /// The background job for updating all flow fields
        /// </summary>
        public JobHandle updatingFlowFields { get; private set; }


        protected NativeArray<JobHandle> m_updatingHandles;
        protected NativeArray<JobHandle> m_movingByArchetype;


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
        public virtual void Initialize(Dictionary<NavAgentArchetype, List<Transform>> agents)
        {
            Dispose();  // clear any existing memory
            int configCount = configurations.Count;
            int archetypeCount = archetypes.Count;

            // initialize agent movement configurations
            m_movementTypes = new NativeArray<AgentMovement>(configCount, Allocator.Persistent);
            for (int i = 0; i < configCount; i++) { m_movementTypes[i] = configurations[i].movement; }

            // initialize job handle arrays
            m_updatingHandles = new NativeArray<JobHandle>(archetypeCount, Allocator.Persistent);
            m_movingByArchetype = new NativeArray<JobHandle>(archetypeCount, Allocator.Persistent);

            // initialize the agent properties
            m_agentMovementTypes = new List<NativeArray<int>>(archetypeCount);
            m_agentKinematics = new List<NativeArray<AgentKinematics>>(archetypeCount);
            m_transformAccess = new List<TransformAccessArray>(archetypeCount);
            m_agentSteering = new List<NativeArray<float3>>(archetypeCount);
            m_agentActive = new List<NativeArray<bool>>(archetypeCount);
            m_totalAgents = new List<int>(archetypeCount);

            // initialize the flow field data structures
            int length = world.size.x * world.size.y * world.size.z;
            m_flowFields = new List<NativeArray<byte>>(archetypeCount);
            m_costFields = new List<NativeArray<byte>>(archetypeCount);
            m_intFields = new List<NativeArray<ushort>>(archetypeCount);
            m_openNodes = new List<NativeQueue<int3>>(archetypeCount);

            // initialize each archetype
            for (int i = 0; i < archetypeCount; i++)
            {
                // initialize flow field data for each archetype
                m_openNodes.Add(new NativeQueue<int3>(Allocator.Persistent));
                m_flowFields.Add(new NativeArray<byte>(length, Allocator.Persistent));
                m_costFields.Add(new NativeArray<byte>(length, Allocator.Persistent));
                m_intFields.Add(new NativeArray<ushort>(length, Allocator.Persistent));

                // initialize agent data for each archetype
                Transform[] transforms = agents[archetypes[i]].ToArray();
                int count = transforms.Length;

                TransformAccessArray transformAccess = new TransformAccessArray(transforms);
                NativeArray<int> movementTypes = new NativeArray<int>(count, Allocator.Persistent);
                NativeArray<float3> steering = new NativeArray<float3>(count, Allocator.Persistent);
                NativeArray<AgentKinematics> kinematics = new NativeArray<AgentKinematics>(count, Allocator.Persistent);
                NativeArray<bool> active = new NativeArray<bool>(count, Allocator.Persistent);

                // read agent configuration
                for (int a = 0; a < count; a++)
                {
                    NavAgent agent = transforms[a].GetComponent<NavAgent>();
                    int index = configurations.IndexOf(agent.configuration);
                    movementTypes[a] = index;
                }

                m_agentMovementTypes.Add(movementTypes);
                m_transformAccess.Add(transformAccess);
                m_agentKinematics.Add(kinematics);
                m_agentSteering.Add(steering);
                m_agentActive.Add(active);
                m_totalAgents.Add(count);
            }

            // initialize the agent spatial map
            m_spatialMap = new NativeMultiHashMap<int3, float3>(m_totalAgents.Sum(), Allocator.Persistent);
            m_spatialMapWriter = m_spatialMap.AsParallelWriter();

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
            int blockCount = blockManager.blocks.Count;
            m_blockTypes = new List<NativeArray<Block>>();
            for (int a = 0; a < archetypeCount; a++)
            {
                NativeArray<Block> blocks = new NativeArray<Block>(
                    blockCount, Allocator.Persistent);

                for (int i = 0; i < blockCount; i++)
                {
                    BlockConfiguration block = blockManager.blocks[i];
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
        }

        /// <summary>
        /// Schedules background jobs to move all agents using the given delta time
        /// </summary>
        public JobHandle MoveAgents(float dt, JobHandle dependsOn = default)
        {
            m_spatialMap.Clear();

            AgentWorld agentWorld = new AgentWorld();
            agentWorld.offset = world.transform.position;
            agentWorld.rotation = world.transform.rotation;
            agentWorld.center = world.data.center;
            agentWorld.scale = world.scale;
            agentWorld.size = new int3(
                world.size.x,
                world.size.y,
                world.size.z
            );

            if (!movingAllAgents.IsCompleted)
                movingAllAgents.Complete();

            // update the spatial map with all agent positions
            JobHandle spatialMaps = UpdateSpatialMap(agentWorld, dependsOn);

            // update each agents position by archetype
            for (int i = 0; i < archetypes.Count; i++)
                m_movingByArchetype[i] = MoveByArchetype(i, agentWorld, dt, spatialMaps);

            movingAllAgents = JobHandle.CombineDependencies(m_movingByArchetype);
            return movingAllAgents;
        }

        /// <summary>
        /// Updates the flow fields for each agent archetype to target the new goal position
        /// </summary>
        public JobHandle UpdateFlowFields(Vector3Int goal, JobHandle dependsOn = default)
        {
            int3 target = new int3(goal.x, goal.y, goal.z);

            if (!updatingFlowFields.IsCompleted)
                updatingFlowFields.Complete();

            for (int i = 0; i < archetypes.Count; i++)
                m_updatingHandles[i] = UpdateFlowField(i, target, dependsOn);

            updatingFlowFields = JobHandle.CombineDependencies(m_updatingHandles);
            return updatingFlowFields;
        }

        /// <summary>
        /// Disposes any unmanaged memory from the agent manager
        /// </summary>
        public void Dispose()
        {
            // dispose the spatial map data
            if (m_spatialMap.IsCreated) { m_spatialMap.Dispose(); }

            // dispose lookup tables
            if (m_directions.IsCreated) { m_directions.Dispose(); }
            if (m_directionsNESW.IsCreated) { m_directionsNESW.Dispose(); }
            if (m_movementTypes.IsCreated) { m_movementTypes.Dispose(); }

            // dispose the flow field data
            if (m_updatingHandles.IsCreated) { m_updatingHandles.Dispose(); }
            if (m_movingByArchetype.IsCreated) { m_movingByArchetype.Dispose(); }

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

            // dispose the agent data
            if (m_agentActive != null)
                foreach (var item in m_agentActive)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_agentSteering != null)
                foreach (var item in m_agentSteering)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_agentKinematics != null)
                foreach (var item in m_agentKinematics)
                    if (item.IsCreated) { item.Dispose(); }

            if (m_transformAccess != null)
                foreach (var item in m_transformAccess)
                    if (item.isCreated) { item.Dispose(); }

            if (m_agentMovementTypes != null)
                foreach (var item in m_agentMovementTypes)
                    if (item.IsCreated) { item.Dispose(); }
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            if (blockManager == null)
                blockManager = GetComponent<BlockManager>();

            if (world == null)
                world = GetComponent<World>();
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
            int length = world.size.x * world.size.y * world.size.z;
            int3 worldSize = new int3(world.size.x, world.size.y, world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = world.data.voxels,
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
        /// Schedules background jobs to update the spatial map for all agents
        /// </summary>
        protected JobHandle UpdateSpatialMap(AgentWorld agentWorld, JobHandle dependsOn = default)
        {
            JobHandle handle = dependsOn;
            for (int i = 0; i < archetypes.Count; i++)
            {
                BuildSpatialMapJob job = new BuildSpatialMapJob();
                job.spatialMap = m_spatialMapWriter;
                job.agents = m_agentKinematics[i];
                job.active = m_agentActive[i];
                job.size = spatialBucketSize;
                job.world = agentWorld;

                handle = job.Schedule(
                    m_transformAccess[i],
                    handle
                );
            }

            return handle;
        }

        /// <summary>
        /// Schedules background jobs to move each agent in the scene by the given delta time
        /// </summary>
        protected JobHandle MoveByArchetype(int index, AgentWorld agentWorld, float dt, JobHandle dependsOn = default)
        {
            FlowFieldSeekJob seekJob = new FlowFieldSeekJob()
            {
                world = agentWorld,
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes[index],

                flowField = m_flowFields[index],
                flowDirections = m_directions,
                flowFieldSize = agentWorld.size,

                active = m_agentActive[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],
            };
            JobHandle seekHandle = seekJob.Schedule(m_totalAgents[index], 1, dependsOn);

            AvoidCollisionBehavior avoidJob = new AvoidCollisionBehavior()
            {
                avoidForce = avoidForce,
                avoidRadius = avoidRadius,
                avoidDistance = avoidDistance,
                maxDepth = maxAvoidDepth,

                world = agentWorld,
                active = m_agentActive[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                spatialMap = m_spatialMap,
                size = spatialBucketSize
            };
            JobHandle avoidHandle = avoidJob.Schedule(m_totalAgents[index], 1, seekHandle);

            QueueBehavior queueJob = new QueueBehavior()
            {
                maxDepth = maxQueueDepth,
                maxBrakeForce = brakeForce,
                maxQueueRadius = queueRadius,
                maxQueueAhead = queueDistance,

                world = agentWorld,
                active = m_agentActive[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                size = spatialBucketSize,
                spatialMap = m_spatialMap
            };
            JobHandle queueHandle = queueJob.Schedule(m_totalAgents[index], 1, avoidHandle);

            ResolveCollisionBehavior collisionJob = new ResolveCollisionBehavior()
            {
                collisionForce = collisionForce,
                collisionRadius = collisionRadius,
                maxDepth = maxCollisionDepth,

                world = agentWorld,
                active = m_agentActive[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                spatialMap = m_spatialMap,
                size = spatialBucketSize
            };
            JobHandle collisionHandle = collisionJob.Schedule(m_totalAgents[index], 1, queueHandle);

            MoveAgentJob moveJob = new MoveAgentJob()
            {
                maxForce = maxForce,
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes[index],

                world = agentWorld,
                active = m_agentActive[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],
                deltaTime = dt,

                flowField = m_flowFields[index],
                flowFieldSize = agentWorld.size,
            };

            return moveJob.Schedule(m_transformAccess[index], collisionHandle);
        }
    }
}
