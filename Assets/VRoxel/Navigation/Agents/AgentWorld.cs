using Unity.Mathematics;
using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// The properties of the world that contain the agents
    /// </summary>
    [Serializable]
    public struct AgentWorld
    {
        /// <summary>
        /// The scale factor for the worlds size
        /// </summary>
        public float scale;

        /// <summary>
        /// The offset of the world in the scene
        /// </summary>
        public float3 offset;

        /// <summary>
        /// The local center coordinates of the world
        /// </summary>
        public float3 center;

        /// <summary>
        /// The orientation of the world in the scene
        /// </summary>
        public quaternion rotation;
    }
}
