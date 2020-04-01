using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Jobs;


namespace VRoxel.Navigation
{
    public class AgentManager
    {
        private List<NavAgent> _agents;
        private TransformAccessArray _agentTransforms;

        public AgentManager()
        {
            _agents = new List<NavAgent>();
            _agentTransforms = new TransformAccessArray();
        }

        /// <summary>
        /// Returns a list of all managed agents
        /// </summary>
        public List<NavAgent> all { get { return _agents; } }

        /// <summary>
        /// Update each agents position in the world
        /// </summary>
        public void MoveAgents(float dt)
        {
            for (int i = 0; i < _agents.Count; i++)
            {
                _agents[i].Move(dt);
            }
        }
    }
}
