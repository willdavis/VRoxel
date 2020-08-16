﻿using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
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
            m_transformAccess = new TransformAccessArray(transforms);
            m_agentKinematics = new NativeArray<AgentKinematics>(
                m_totalAgents, Allocator.Persistent);
        }

        /// <summary>
        /// Schedules a background job to move all agents using the given delta time
        /// </summary>
        public JobHandle MoveAgents(float dt, JobHandle dependsOn = default(JobHandle))
        {
            return m_movingAgents;
        }

        /// <summary>
        /// Schedules a background job to update all flow fields to target the given goal
        /// </summary>
        public JobHandle UpdatePathfinding(Vector3Int goal, JobHandle dependsOn = default(JobHandle))
        {
            return m_updatingPathfinding;
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
    }
}
