using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

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
        /// The async transform access to each agent's transform
        /// </summary>
        protected TransformAccessArray m_transformAccess;

        /// <summary>
        /// The background job to move each agent in the scene
        /// </summary>
        protected JobHandle m_movingAgents;

        /// <summary>
        /// The background job to update each of the flow fields
        /// </summary>
        protected JobHandle m_updatingPathfinding;

        /// <summary>
        /// The reference to the voxel world's transform and center point
        /// </summary>
        protected AgentWorld m_worldProperties;

        /// <summary>
        /// Caches the total number of agents being managed
        /// </summary>
        protected int m_totalAgents;


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
        /// Initializes the agent manager with a world and an array of agent transforms
        /// </summary>
        public virtual void Initialize(AgentWorld worldProperties, Transform[] transforms)
        {
            Dispose();  // clear any existing memory
            m_totalAgents = transforms.Length;
            m_worldProperties = worldProperties;

            // initialize the agent properties
            m_transformAccess = new TransformAccessArray(transforms);
            m_agentKinematics = new NativeArray<AgentKinematics>(
                m_totalAgents, Allocator.Persistent);

            // initialize the agent spatial map
            m_spatialMap = new NativeMultiHashMap<int3, float3>(
                m_totalAgents, Allocator.Persistent);
            m_spatialMapWriter = m_spatialMap.AsParallelWriter();

            int3 size = worldProperties.size;
            int length = size.x * size.y * size.z;

            // initialize the flow field data structures
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
            int3 worldSize = m_worldProperties.size;
            int3 target = new int3(goal.x, goal.y, goal.z);
            int length = worldSize.x * worldSize.y * worldSize.z;

            if (!m_updatingPathfinding.IsCompleted)
                m_updatingPathfinding.Complete();

            return SchedulePathfindingUpdate(target, length, dependsOn);
        }

        /// <summary>
        /// Disposes any unmanaged memory from the agent manager
        /// </summary>
        public void Dispose()
        {
            // dispose the agent data
            if (m_agentKinematics.IsCreated) { m_agentKinematics.Dispose(); }
            if (m_transformAccess.isCreated) { m_transformAccess.Dispose(); }

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

        protected JobHandle SchedulePathfindingUpdate(int3 target, int length, JobHandle dependsOn = default)
        {
            /*
            UpdateCostFieldJob costJob = new UpdateCostFieldJob();
            costJob.size = m_worldProperties.size;
            costJob.voxels = new NativeArray<byte>(1, Allocator.Temp);
            costJob.directionMask = new NativeArray<int>(1, Allocator.Temp);
            costJob.directions = new NativeArray<int3>(1, Allocator.Temp);
            costJob.costField = new NativeArray<byte>(1, Allocator.Temp);
            costJob.blocks = new NativeArray<Block>(1, Allocator.Temp);
            costJob.height = 1;
            JobHandle costHandle = costJob.Schedule(length, 1, dependsOn);

            ClearIntFieldJob clearJob = new ClearIntFieldJob();
            clearJob.intField = new NativeArray<ushort>(1, Allocator.Temp);
            JobHandle clearHandle = clearJob.Schedule(length, 1, costHandle);

            UpdateIntFieldJob intJob = new UpdateIntFieldJob();
            intJob.directions = new NativeArray<int3>(1, Allocator.Temp);
            intJob.costField = new NativeArray<byte>(1, Allocator.Temp);
            intJob.intField = new NativeArray<ushort>(1, Allocator.Temp);
            intJob.size = m_worldProperties.size;
            intJob.open = m_openNodes;
            intJob.goal = target;
            JobHandle intHandle = intJob.Schedule(clearHandle);

            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob();
            flowJob.directions = new NativeArray<int3>(1, Allocator.Temp);
            flowJob.flowField = new NativeArray<byte>(1, Allocator.Temp);
            flowJob.intField = new NativeArray<ushort>(1, Allocator.Temp);
            flowJob.size = m_worldProperties.size;

            m_updatingPathfinding = flowJob.Schedule(length, 1, intHandle);
            */
            return m_updatingPathfinding;
        }

        protected JobHandle ScheduleAgentMovement(float dt, JobHandle dependsOn = default)
        {
            /*
            BuildSpatialMapJob spaceJob = new BuildSpatialMapJob();
            spaceJob.spatialMap = m_spatialMapWriter;
            //spaceJob.agents = m_agentKinematics;
            //spaceJob.world = m_worldProperties;
            spaceJob.size = new int3(1,1,1); // bucket size
            JobHandle spaceHandle = spaceJob.Schedule(m_transformAccess, dependsOn);

            FlowFieldSeekJob seekJob = new FlowFieldSeekJob();
            seekJob.steering = new NativeArray<float3>(1, Allocator.Temp);
            seekJob.flowDirections = new NativeArray<int3>(1, Allocator.Temp);
            seekJob.flowField = new NativeArray<byte>(1, Allocator.Temp);
            seekJob.flowFieldSize = m_worldProperties.size;
            //spaceJob.agents = m_agentKinematics;
            //seekJob.world = m_worldProperties;
            seekJob.maxSpeed = 1f;
            JobHandle seekHandle = seekJob.Schedule(m_totalAgents, 1, spaceHandle);

            AvoidCollisionBehavior avoidJob = new AvoidCollisionBehavior();
            avoidJob.steering = new NativeArray<float3>(1, Allocator.Temp);
            avoidJob.avoidForce = 1f;
            avoidJob.avoidRadius = 1f;
            avoidJob.avoidDistance = 1f;
            avoidJob.maxDepth = 1;
            //avoidJob.agents = m_agentKinematics;
            //avoidJob.world = m_worldProperties;
            avoidJob.size = new int3(1,1,1); // bucket size
            avoidJob.spatialMap = m_spatialMap;
            JobHandle avoidHandle = avoidJob.Schedule(m_totalAgents, 1, seekHandle);

            QueueBehavior queueJob = new QueueBehavior();
            queueJob.steering = new NativeArray<float3>(1, Allocator.Temp);
            //queueJob.agents = m_agentKinematics;
            //queueJob.world = m_worldProperties;
            queueJob.size = new int3(1,1,1); // bucket size
            queueJob.spatialMap = m_spatialMap;
            queueJob.maxDepth = 1;
            queueJob.maxBrakeForce = 1f;
            queueJob.maxQueueRadius = 1f;
            queueJob.maxQueueAhead = 1f;
            JobHandle queueHandle = queueJob.Schedule(m_totalAgents, 1, avoidHandle);

            ResolveCollisionBehavior collisionJob = new ResolveCollisionBehavior();
            collisionJob.steering = new NativeArray<float3>(1, Allocator.Temp);
            //collisionJob.agents = m_agentKinematics;
            //collisionJob.world = m_worldProperties;
            collisionJob.size = new int3(1,1,1); // bucket size
            collisionJob.spatialMap = m_spatialMap;
            collisionJob.maxDepth = 1;
            collisionJob.collisionForce = 1f;
            collisionJob.collisionRadius = 1f;
            JobHandle collisionHandle = collisionJob.Schedule(m_totalAgents, 1, queueHandle);

            MoveAgentJob moveJob = new MoveAgentJob();
            collisionJob.steering = new NativeArray<float3>(1, Allocator.Temp);
            //collisionJob.agents = m_agentKinematics;
            //collisionJob.world = m_worldProperties;
            moveJob.flowField = new NativeArray<byte>(1, Allocator.Temp);
            moveJob.flowFieldSize = m_worldProperties.size;
            moveJob.deltaTime = dt;
            moveJob.mass = 1f;
            moveJob.maxForce = 1f;
            moveJob.maxSpeed = 1f;
            moveJob.turnSpeed = 1f;

            m_movingAgents = moveJob.Schedule(m_transformAccess, collisionHandle);
            */
            return m_movingAgents;
        }
    }
}
