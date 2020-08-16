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
        }

        /// <summary>
        /// Schedules background jobs to move all agents using the given delta time
        /// </summary>
        public JobHandle MoveAgents(float dt, JobHandle dependsOn = default(JobHandle))
        {
            m_spatialMap.Clear();

            if (!m_movingAgents.IsCompleted)
                m_movingAgents.Complete();

            return ScheduleAgentMovement(dt, dependsOn);
        }

        /// <summary>
        /// Schedules a background job to update all flow fields to target the given goal
        /// </summary>
        public JobHandle UpdatePathfinding(Vector3Int goal, JobHandle dependsOn = default(JobHandle))
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
            if (m_agentKinematics.IsCreated)
                m_agentKinematics.Dispose();

            if (m_transformAccess.isCreated)
                m_transformAccess.Dispose();

            if (m_spatialMap.IsCreated)
                m_spatialMap.Dispose();

            if (m_openNodes.IsCreated)
                m_openNodes.Dispose();
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            m_openNodes = new NativeQueue<int3>(Allocator.Persistent);
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        #endregion
        //-------------------------------------------------

        protected JobHandle SchedulePathfindingUpdate(int3 target, int length, JobHandle dependsOn = default(JobHandle))
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

        protected JobHandle ScheduleAgentMovement(float dt, JobHandle dependsOn = default(JobHandle))
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
