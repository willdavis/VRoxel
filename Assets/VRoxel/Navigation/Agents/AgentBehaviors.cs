using System;

namespace VRoxel.Navigation.Agents
{
    /// <summary>
    /// A bitmask that describes the
    /// navigation behaviors for an agent
    /// </summary>
    [Serializable] [Flags]
    public enum AgentBehaviors
    {
        /// <summary>
        /// None of the agent's navigation behaviors are enabled
        /// </summary>
        None = 0,

        /// <summary>
        /// The agent is active and initialized in the scene
        /// </summary>
        Active = (1 << 0),

        /// <summary>
        /// The agent is moving towards their goal
        /// </summary>
        Seeking = (1 << 1),

        /// <summary>
        /// The agent is braking to slow down
        /// </summary>
        Stopping = (1 << 2),

        /// <summary>
        /// The agent is avoiding other agents
        /// </summary>
        Avoiding = (1 << 3),

        /// <summary>
        /// The agent is queueing behind other agents
        /// </summary>
        Queueing = (1 << 4),
    }
}