﻿using System.Collections.Generic;
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
        /// the minimum cost difference between two nodes.
        /// Prevents overlap due to small cost differences
        /// </summary>
        public int minCostDifference;

        /// <summary>
        /// The size of the spatial buckets used to parition the map
        /// </summary>
        public int3 spatialBucketSize;

        [Header("Movement Settings")]
        public Vector3 gravity;
        public float maxForce;

        [Header("Collision Detection")]
        [Tooltip("The minimum distance required to detect a collision")]
        public float minCollisionDistance = 0.01f;
        [Tooltip("The minimum force required to separate agents (deadband)")]
        public float minCollisionForce = 0.01f;

        [Tooltip("The maximum number of agents to check when resolving collisions")]
        public int maxCollisionDepth = 100;

        [Header("Queuing Behavior")]
        public float brakeForce;
        public float queueRadius;
        public float queueDistance;
        public int maxQueueDepth;

        [Header("Avoidance Behavior")]
        public float avoidForce;
        public float avoidRadius;
        public float avoidDistance;
        public int maxAvoidDepth;

        /// <summary>
        /// The current navigation behavior flags for each agent, grouped by archetype
        /// </summary>
        public List<NativeArray<AgentBehaviors>> agentBehaviors { get { return m_agentBehaviors; } }

        /// <summary>
        /// The current navigation behavior flags for each agent, grouped by archetype
        /// </summary>
        protected List<NativeArray<AgentBehaviors>> m_agentBehaviors;

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
        /// Cached reference to the different target positions on the flow field
        /// </summary>
        protected NativeList<int3> m_targetPositions;

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


        protected NativeMultiHashMap<int3, SpatialMapData> m_spatialMap;
        protected NativeMultiHashMap<int3, SpatialMapData>.ParallelWriter m_spatialMapWriter;

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
            m_agentBehaviors = new List<NativeArray<AgentBehaviors>>(archetypeCount);
            m_agentKinematics = new List<NativeArray<AgentKinematics>>(archetypeCount);
            m_transformAccess = new List<TransformAccessArray>(archetypeCount);
            m_agentSteering = new List<NativeArray<float3>>(archetypeCount);
            m_totalAgents = new List<int>(archetypeCount);

            // initialize the flow field data structures
            int length = world.size.x * world.size.y * world.size.z;
            m_flowFields = new List<NativeArray<byte>>(archetypeCount);
            m_costFields = new List<NativeArray<byte>>(archetypeCount);
            m_intFields = new List<NativeArray<ushort>>(archetypeCount);
            m_openNodes = new List<NativeQueue<int3>>(archetypeCount);

            // cached data to build the flow fields
            m_targetPositions = new NativeList<int3>(Allocator.Persistent);

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
                NativeArray<AgentBehaviors> behaviors = new NativeArray<AgentBehaviors>(count, Allocator.Persistent);

                // read agent configuration
                for (int a = 0; a < count; a++)
                {
                    NavAgent agent = transforms[a].GetComponent<NavAgent>();
                    int movementType = configurations.IndexOf(agent.configuration);

                    AgentKinematics agentKinematics = new AgentKinematics();
                    agentKinematics.maxSpeed = agent.configuration.movement.topSpeed;

                    movementTypes[a] = movementType;
                    kinematics[a] = agentKinematics;
                }

                m_agentMovementTypes.Add(movementTypes);
                m_transformAccess.Add(transformAccess);
                m_agentKinematics.Add(kinematics);
                m_agentBehaviors.Add(behaviors);
                m_agentSteering.Add(steering);
                m_totalAgents.Add(count);
            }

            // initialize the agent spatial map
            m_spatialMap = new NativeMultiHashMap<int3, SpatialMapData>(m_totalAgents.Sum(), Allocator.Persistent);
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
        /// Updates the flow fields for each agent archetype to target the new goal position(s)
        /// </summary>
        public JobHandle UpdateFlowFields(List<Vector3Int> targets, JobHandle dependsOn = default)
        {
            if (!updatingFlowFields.IsCompleted)
                updatingFlowFields.Complete();

            // update the target positions
            m_targetPositions.Clear();
            foreach (var t in targets)
                m_targetPositions.Add(new int3(t.x, t.y, t.z));

            // schedule jobs to update each archetype
            for (int i = 0; i < archetypes.Count; i++)
                m_updatingHandles[i] = UpdateFlowField(i, dependsOn);

            updatingFlowFields = JobHandle.CombineDependencies(m_updatingHandles);
            return updatingFlowFields;
        }

        /// <summary>
        /// Returns the current max speed of an agent
        /// </summary>
        public float MaxSpeed(NavAgent agent)
        {
            if (m_agentKinematics == null)
                return agent.configuration.movement.topSpeed;

            movingAllAgents.Complete();

            int archetypeIndex = archetypes.IndexOf(agent.configuration.archetype);
            return m_agentKinematics[archetypeIndex][agent.index].maxSpeed;
        }

        /// <summary>
        /// Changes the current max speed of an agent
        /// </summary>
        public void SetMaxSpeed(NavAgent agent, float maxSpeed)
        {
            movingAllAgents.Complete();

            int archetypeIndex = archetypes.IndexOf(agent.configuration.archetype);
            AgentKinematics kinematics = m_agentKinematics[archetypeIndex][agent.index];
            kinematics.maxSpeed = maxSpeed;

            NativeArray<AgentKinematics> agentKinematics = m_agentKinematics[archetypeIndex];
            agentKinematics[agent.index] = kinematics;
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
            if (m_targetPositions.IsCreated) { m_targetPositions.Dispose(); }
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
            if (m_agentBehaviors != null)
                foreach (var item in m_agentBehaviors)
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
        /// Schedules background jobs to update the flow field to target the new goal positions
        /// </summary>
        protected JobHandle UpdateFlowField(int archetype, JobHandle dependsOn = default)
        {
            int height = archetypes[archetype].collision.height;
            int length = world.size.x * world.size.y * world.size.z;
            int3 worldSize = new int3(world.size.x, world.size.y, world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = world.data.voxels,
                directionMask = m_directionsNESW,
                directions = m_directions,
                costField = m_costFields[archetype],
                blocks = m_blockTypes[archetype],
                size = worldSize,
                height = height
            };
            JobHandle costHandle = costJob.Schedule(length, 1, dependsOn);


            ClearIntFieldJob clearJob = new ClearIntFieldJob()
            {
                intField = m_intFields[archetype]
            };
            JobHandle clearHandle = clearJob.Schedule(length, 1, costHandle);


            UpdateIntFieldJob intJob = new UpdateIntFieldJob()
            {
                directions = m_directions,
                minCostDiff = minCostDifference,
                costField = m_costFields[archetype],
                intField = m_intFields[archetype],
                open = m_openNodes[archetype],
                targets = m_targetPositions,
                size = worldSize,
            };
            JobHandle intHandle = intJob.Schedule(clearHandle);


            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob()
            {
                directions = m_directions,
                flowField = m_flowFields[archetype],
                intField = m_intFields[archetype],
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
                job.movementConfigs = m_movementTypes;
                job.movement = m_agentMovementTypes[i];
                job.collision = archetypes[i].collision;
                job.spatialMap = m_spatialMapWriter;
                job.behaviors = m_agentBehaviors[i];
                job.agents = m_agentKinematics[i];
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

                behaviors = m_agentBehaviors[index],
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
                behaviors = m_agentBehaviors[index],
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
                behaviors = m_agentBehaviors[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                size = spatialBucketSize,
                spatialMap = m_spatialMap
            };
            JobHandle queueHandle = queueJob.Schedule(m_totalAgents[index], 1, avoidHandle);

            StoppingBehavior stopJob = new StoppingBehavior()
            {
                maxBrakeForce = brakeForce,
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],
                behaviors = m_agentBehaviors[index]
            };
            JobHandle stopHandle = stopJob.Schedule(m_totalAgents[index], 1, queueHandle);

            CollisionBehavior collisionJob = new CollisionBehavior()
            {
                movementConfigs = m_movementTypes,
                movement = m_agentMovementTypes[index],
                collision = archetypes[index].collision,
                minDistance = minCollisionDistance,
                minForce = minCollisionForce,
                maxDepth = maxCollisionDepth,

                world = agentWorld,
                behaviors = m_agentBehaviors[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                spatialMap = m_spatialMap,
                size = spatialBucketSize
            };
            JobHandle collisionHandle = collisionJob.Schedule(m_totalAgents[index], 1, stopHandle);

            GravityBehavior gravityJob = new GravityBehavior()
            {
                world = agentWorld,
                behaviors = m_agentBehaviors[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],

                blocks = m_blockTypes[index],
                voxels = world.data.voxels,
                gravity = new float3(
                    gravity.x,
                    gravity.y,
                    gravity.z
                ),
            };
            JobHandle gravityHandle = gravityJob.Schedule(m_totalAgents[index], 1, collisionHandle);

            MoveAgentJob moveJob = new MoveAgentJob()
            {
                maxForce = maxForce,
                movementTypes = m_movementTypes,
                agentMovement = m_agentMovementTypes[index],

                world = agentWorld,
                behaviors = m_agentBehaviors[index],
                agents = m_agentKinematics[index],
                steering = m_agentSteering[index],
                deltaTime = dt,

                flowField = m_flowFields[index],
                flowFieldSize = agentWorld.size,
            };

            return moveJob.Schedule(m_transformAccess[index], gravityHandle);
        }
    }
}
