using VRoxel.Navigation.Agents;
using UnityEngine;

namespace VRoxel.Navigation.Data
{
    /// <summary>
    /// Data container for NavAgent settings
    /// </summary>
    [CreateAssetMenu(fileName = "NavAgentArchetype.asset", menuName = "VRoxel/Navigation/NavAgent Archetype", order = 1)]
    public class NavAgentArchetype : ScriptableObject
    {
        /// <summary>
        /// The name of this archetype for display in the UI
        /// </summary>
        public new string name;

        /// <summary>
        /// The description of this archetype for display in the UI
        /// </summary>
        [Multiline] public string description;

        /// <summary>
        /// The properties common to all agents of this archetype
        /// </summary>
        public AgentArchetype configuration;
    }
}