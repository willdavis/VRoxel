using System.Collections.Generic;
using VRoxel.Navigation.Agents;
using VRoxel.Navigation.Data;

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A component to help manage NavAgents in the scene
    /// </summary>
    public class NavAgentManager : MonoBehaviour
    {
        /// <summary>
        /// The configurations for each type of NavAgent that should be managed
        /// </summary>
        public List<NavAgentConfiguration> configurations;



        /// <summary>
        /// The current kinematic properties of each managed agent
        /// </summary>
        protected NativeArray<AgentKinematics> m_agentKinematics;

        /// <summary>
        /// The async transform access to each managed agents transform
        /// </summary>
        protected TransformAccessArray m_transformAccess;



        //-------------------------------------------------
        #region Monobehaviors



        #endregion
        //-------------------------------------------------
    }
}
