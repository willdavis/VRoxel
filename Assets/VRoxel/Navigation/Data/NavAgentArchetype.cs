using System.Collections.Generic;
using System;

using VRoxel.Navigation.Agents;
using VRoxel.Core.Data;

using UnityEngine;

namespace VRoxel.Navigation.Data
{
    /// <summary>
    /// Data container for agent archetype settings
    /// </summary>
    [CreateAssetMenu(
        fileName = "NavAgentArchetype.asset",
        menuName = "VRoxel/Navigation/NavAgent Archetype",
        order = 1)]
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
        /// The collision properties of this archetype
        /// </summary>
        public AgentCollision collision;

        /// <summary>
        /// The movement costs for the different block configurations
        /// </summary>
        [Tooltip("Add blocks to change their movement cost for this archetype")]
        public List<MovementCost> movementCosts;
    }

    /// <summary>
    /// The movement costs for the different block configurations
    /// </summary>
    [Serializable]
    public struct MovementCost
    {
        public BlockConfiguration block;
        public byte cost;
    }
}