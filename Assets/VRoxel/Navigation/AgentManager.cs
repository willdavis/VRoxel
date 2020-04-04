using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Core;

namespace VRoxel.Navigation
{
    public class AgentManager
    {
        private World _world;

        private int _max;
        private List<NavAgent> _agents;
        private TransformAccessArray _transformAccess;
        private NativeArray<Vector3> _agentDirections;

        NativeArray<byte> _flowField;
        NativeArray<Vector3Int> _flowDirections;

        public AgentManager(World world, int maxAgents)
        {
            _world = world;
            _max = maxAgents;
            _agents = new List<NavAgent>(maxAgents);
            _agentDirections = new NativeArray<Vector3>(maxAgents, Allocator.Persistent);

            int size = world.size.x * world.size.y * world.size.z;
            _flowField = new NativeArray<byte>(size, Allocator.Persistent);
            _flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);
        }

        /// <summary>
        /// Dispose all unmanaged memory from the AgentManager
        /// </summary>
        public void Dispose()
        {
            _transformAccess.Dispose();
            _agentDirections.Dispose();

            _flowField.Dispose();
            _flowDirections.Dispose();
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
                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                flowField = _flowField,
                flowDirections = _flowDirections,

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
