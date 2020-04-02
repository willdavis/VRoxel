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
        private int _max;
        private List<NavAgent> _agents;
        private Transform[] _agentTransforms;
        private TransformAccessArray _agentTransformsAccess;
        private NativeArray<Vector3> _agentDirections;

        public AgentManager(int max)
        {
            _max = max;
            _agents = new List<NavAgent>(max);
            _agentTransforms = new Transform[max];
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
                if (!_agents[i].isActiveAndEnabled)
                    continue;

                _agents[i].Move(dt);
            }
        }

        /// <summary>
        /// Update each agents position in the world
        /// asynchronously using Unity jobs
        /// </summary>
        public void MoveAgentsAsync(float dt)
        {
            _agentTransformsAccess = new TransformAccessArray(_agentTransforms);
            _agentDirections = new NativeArray<Vector3>(_max, Allocator.TempJob);

            MoveAgentJob job = new MoveAgentJob()
            {
                speed = 1f,
                deltaTime = dt,
                directions = _agentDirections
            };
            JobHandle handle = job.Schedule(_agentTransformsAccess);
            handle.Complete();

            _agentTransformsAccess.Dispose();
            _agentDirections.Dispose();
        }
    }
}
