using VRoxel.Navigation.Data;
using UnityEngine;

namespace VRoxel.Navigation
{
    /// <summary>
    /// A component to provide an agent with navigational properties
    /// </summary>
    public class NavAgent : MonoBehaviour
    {
        /// <summary>
        /// The agents index in the background job arrays
        /// </summary>
        [HideInInspector] public int index;

        /// <summary>
        /// The configuration settings for this type of agent
        /// </summary>
        public NavAgentConfiguration configuration;
    }
}
