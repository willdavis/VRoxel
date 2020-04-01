using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Jobs;
using Unity.Collections;


namespace VRoxel.Navigation
{
    public class AgentManager
    {
        private List<NavAgent> _agents;
        private TransformAccessArray _agentTransforms;
        private NativeArray<Vector3> _agentDirections;

        public AgentManager()
        {
            _agents = new List<NavAgent>(1000);
            _agentTransforms = new TransformAccessArray();
            _agentDirections = new NativeArray<Vector3>(1000, Allocator.Persistent);
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
