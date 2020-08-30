using UnityEngine;
using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// Describes the static properties of an agent's collision cylinder
    /// </summary>
    [Serializable]
    public struct AgentCollision
    {
        /// <summary>
        /// The voxel height of this agent
        /// </summary>
        [Tooltip("the voxel height of this agent")]
        public int height;

        /// <summary>
        /// The voxel radius of this agent
        /// </summary>
        [Tooltip("the voxel collision radius of this agent")]
        public float radius;
    }
}