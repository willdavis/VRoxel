using Unity.Mathematics;
using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// Describes the current position and velocity of an agent in the scene
    /// </summary>
    [Serializable]
    public struct AgentKinematics
    {
        public bool active;
        public float3 position;
        public float3 velocity;
    }
}
