using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// Describes the static movement properties of an agent
    /// </summary>
    [Serializable]
    public struct AgentMovement
    {
        public float mass;
        public float topSpeed;
        public float turnSpeed;
    }
}