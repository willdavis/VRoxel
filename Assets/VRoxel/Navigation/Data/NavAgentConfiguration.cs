using UnityEngine;
using System;

namespace VRoxel.Navigation.Data
{
    /// <summary>
    /// Data container for NavAgent settings
    /// </summary>
    [CreateAssetMenu(fileName = "NavAgentConfiguration.asset", menuName = "VRoxel/Navigation/NavAgent Configuration", order = 1)]
    public class NavAgentConfiguration : ScriptableObject
    {
        /// <summary>
        /// The properties needed to move the agent in the scene
        /// </summary>
        public AgentMotion movement;
    }

    /// <summary>
    /// Data container for NavAgent movement settings
    /// </summary>
    [Serializable]
    public struct AgentMotion
    {
        public float mass;
        public float topSpeed;
        public float turnSpeed;
    }
}