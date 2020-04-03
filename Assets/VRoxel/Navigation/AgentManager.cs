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
        private TransformAccessArray _transformAccess;
        private NativeArray<Vector3> _agentDirections;

        public AgentManager(int max)
        {
            _max = max;
            _agents = new List<NavAgent>(max);
            _agentDirections = new NativeArray<Vector3>(_max, Allocator.Persistent);
        }

        /// <summary>
        /// Dispose all unmanaged memory from the AgentManager
        /// </summary>
        public void Dispose()
        {
            _transformAccess.Dispose();
            _agentDirections.Dispose();
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
        public JobHandle MoveAgentsAsync(float dt)
        {
            FlowDirectionJob flowJob = new FlowDirectionJob()
            {
                directions = _agentDirections
            };

            MoveAgentJob moveJob = new MoveAgentJob()
            {
                speed = 1f,
                deltaTime = dt,
                directions = _agentDirections
            };

            JobHandle flowHandle = flowJob.Schedule(_transformAccess);
            return moveJob.Schedule(_transformAccess, flowHandle);
        }

        public void TransformAccess(Transform[] transforms)
        {
            _transformAccess = new TransformAccessArray(transforms);
        }
    }
}
