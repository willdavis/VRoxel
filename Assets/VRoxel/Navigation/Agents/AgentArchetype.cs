using UnityEngine;
using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// Describes the static properties of an agent archetype
    /// </summary>
    [Serializable]
    public struct AgentArchetype
    {
        /// <summary>
        /// The voxel height for agents of this archetype
        /// </summary>
        [Tooltip("the voxel height of this archetype")]
        public int height;

        /// <summary>
        /// The voxel collision radius for agents of this archetype
        /// </summary>
        [Tooltip("the voxel collision radius of this archetype")]
        public float radius;
    }
}