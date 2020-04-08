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
        NativeArray<byte> _costField;
        NativeArray<ushort> _intField;
        NativeArray<Block> _blockData;
        NativeArray<Vector3Int> _directions;

        public AgentManager(World world, int maxAgents)
        {
            _world = world;
            _max = maxAgents;
            _agents = new List<NavAgent>(maxAgents);
            _agentDirections = new NativeArray<Vector3>(maxAgents, Allocator.Persistent);

            int size = world.size.x * world.size.y * world.size.z;
            _flowField = new NativeArray<byte>(size, Allocator.Persistent);
            _costField = new NativeArray<byte>(size, Allocator.Persistent);
            _intField = new NativeArray<ushort>(size, Allocator.Persistent);

            _directions = new NativeArray<Vector3Int>(27, Allocator.Persistent);
            for (int i = 0; i < 27; i++)
                _directions[i] = Direction3Int.Directions[i];

            int blockCount = _world.blocks.library.Keys.Count;
            _blockData = new NativeArray<Block>(blockCount, Allocator.Persistent);
            foreach (VRoxel.Core.Block block in _world.blocks.library.Values)
            {
                Block data = new Block();
                data.solid = block.isSolid;

                if (data.solid)
                    data.cost = 1;
                else
                    data.cost = 2;

                _blockData[block.index] = data;
                Debug.Log("added block with id:" + block.index
                    + ", solid: " + _blockData[block.index].solid 
                    + ", cost: " + _blockData[block.index].cost
                );
            }
        }

        /// <summary>
        /// Dispose all unmanaged memory from the AgentManager
        /// </summary>
        public void Dispose()
        {
            _transformAccess.Dispose();
            _agentDirections.Dispose();


            _intField.Dispose();
            _flowField.Dispose();
            _costField.Dispose();
            _blockData.Dispose();
            _directions.Dispose();
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
                flowDirections = _directions,
                flowFieldSize = _world.size,

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

        public JobHandle UpdateFlowField(Vector3Int goal)
        {
            int size = _world.size.x * _world.size.y * _world.size.z;
            Vector3Int worldSize = new Vector3Int(_world.size.x, _world.size.y, _world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = _world.data.voxels,
                directions = _directions,
                costField = _costField,
                blocks = _blockData,
                size = worldSize,
                height = 1
            };
            JobHandle costHandle = costJob.Schedule(size, 1);


            ClearIntFieldJob clearJob = new ClearIntFieldJob()
            {
                intField = _intField
            };
            JobHandle clearHandle = clearJob.Schedule(size, 1, costHandle);


            UpdateIntFieldJob intJob = new UpdateIntFieldJob()
            {
                directions = _directions,
                costField = _costField,
                intField = _intField,
                size = worldSize,
                goal = goal
            };
            JobHandle intHandle = intJob.Schedule(clearHandle);


            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob()
            {
                directions = _directions,
                flowField = _flowField,
                intField = _intField,
                size = worldSize,
            };
            return flowJob.Schedule(size, 1, intHandle);
        }

        public void TransformAccess(Transform[] transforms)
        {
            _transformAccess = new TransformAccessArray(transforms);
        }
    }
}
