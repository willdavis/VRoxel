using VRoxel.Navigation.Agents;
using UnityEngine;

namespace VRoxel.Navigation.Data
{
    /// <summary>
    /// Data container for NavAgent settings
    /// </summary>
    [CreateAssetMenu(fileName = "NavAgentConfiguration.asset", menuName = "VRoxel/Navigation/NavAgent Configuration", order = 1)]
    public class NavAgentConfiguration : ScriptableObject
    {
        /// <summary>
        /// The properties required to move the agent in the scene
        /// </summary>
        public AgentMovement movement;
    }
}