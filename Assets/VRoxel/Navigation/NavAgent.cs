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
        /// The agents index in the agent manager
        /// </summary>
        [HideInInspector] public int index;

        /// <summary>
        /// The current agent manager that controls this agent
        /// </summary>
        [HideInInspector] public NavAgentManager agentManager;

        /// <summary>
        /// The configuration settings for this type of agent
        /// </summary>
        public NavAgentConfiguration configuration;

        /// <summary>
        /// The current max speed of this agent
        /// </summary>
        public float speed
        {
            get { return agentManager.MaxSpeed(this);    }
            set { agentManager.SetMaxSpeed(this, value); }
        }
    }
}
