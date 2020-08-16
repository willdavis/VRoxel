using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A component to help manage NavAgents in the scene
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

        //-------------------------------------------------
        #region Public API

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
        /// Creates a new TransformAccessArray to asynchronously
        /// access each of the given transforms
        /// </summary>
        public void SetTransformAccess(Transform[] transforms)
        {
            m_transformAccess = new TransformAccessArray(transforms);
        }

        /// <summary>
        /// Disposes all unmanaged memory from the agent manager
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



        #endregion
        //-------------------------------------------------
    }
}
