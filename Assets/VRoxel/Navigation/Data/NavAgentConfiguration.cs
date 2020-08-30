using VRoxel.Navigation.Agents;
using UnityEngine;

namespace VRoxel.Navigation.Data
{
    /// <summary>
    /// Data container for navigation agent settings
    /// </summary>
    [CreateAssetMenu(
        fileName = "NavAgentConfiguration.asset",
        menuName = "VRoxel/Navigation/NavAgent Configuration",
        order = 1)]
    public class NavAgentConfiguration : ScriptableObject
    {
        /// <summary>
        /// The agent archetype used for this configuration
        /// </summary>
        public NavAgentArchetype archetype;

        /// <summary>
        /// The properties required to move the agents in the scene
        /// </summary>
        public AgentMovement movement;
    }
}