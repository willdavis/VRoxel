using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;


namespace VRoxel.Navigation
{
    public class AgentManager
    {
        private List<NavAgent> _agents;
        private Transform[] _agentTransforms;
        private TransformAccessArray _agentTransformsAccess;
        private NativeArray<Vector3> _agentDirections;

        public AgentManager()
        {
            _agents = new List<NavAgent>(1000);
            _agentTransforms = new Transform[1000];
            _agentTransformsAccess = new TransformAccessArray(_agentTransforms);
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
            // old version
            for (int i = 0; i < _agents.Count; i++)
            {
                _agents[i].Move(dt);
            }

            // new version
            MoveAgentJob job = new MoveAgentJob()
            {
                speed = 1f,
                deltaTime = Time.deltaTime,
                directions = _agentDirections
            };
            JobHandle handle = job.Schedule(_agentTransformsAccess);
            handle.Complete();
        }
    }
}
