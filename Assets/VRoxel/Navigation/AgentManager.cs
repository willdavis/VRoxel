using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;

namespace VRoxel.Navigation
{
    public class AgentManager
    {
        public int3 spatialBucketSize;

        // agent settings
        public int height;
        public float mass;
        public float maxSpeed;
        public float turnSpeed;

        // moving
        public float moveForce;

        // queuing
        public float brakeForce;
        public float queueRadius;
        public float queueDistance;

        // avoidance
        public float avoidForce;
        public float avoidRadius;
        public float avoidDistance;

        public NativeArray<bool> activeAgents { get { return _agentActive; } }
        public TransformAccessArray transforms { get { return _transformAccess; } }

        World _world;
        int _max;

        TransformAccessArray _transformAccess;
        NativeArray<float3> _agentDirections;
        NativeArray<float3> _agentPositions;
        NativeArray<float3> _agentVelocity;
        NativeArray<bool> _agentActive;

        NativeMultiHashMap<int3, float3> _agentSpatialMap;
        NativeMultiHashMap<int3, float3>.ParallelWriter _agentSpatialMapWriter;

        NativeQueue<int3> _openList;
        NativeArray<byte> _flowField;
        NativeArray<byte> _costField;
        NativeArray<ushort> _intField;
        NativeArray<Block> _blockData;
        NativeArray<int3> _directions;
        NativeArray<int> _directionsNESW;

        JobHandle updateHandle;

        public AgentManager(World world, int maxAgents)
        {
            _world = world;
            _max = maxAgents;

            // initialize agent data structures
            _agentDirections = new NativeArray<float3>(maxAgents, Allocator.Persistent);
            _agentPositions = new NativeArray<float3>(maxAgents, Allocator.Persistent);
            _agentVelocity = new NativeArray<float3>(maxAgents, Allocator.Persistent);
            _agentActive = new NativeArray<bool>(maxAgents, Allocator.Persistent);

            // initialize collision detection & avoidance data structures
            _agentSpatialMap = new NativeMultiHashMap<int3, float3>(maxAgents, Allocator.Persistent);
            _agentSpatialMapWriter = _agentSpatialMap.AsParallelWriter();

            // initialize pathfinding data structures
            int size = world.size.x * world.size.y * world.size.z;
            _flowField = new NativeArray<byte>(size, Allocator.Persistent);
            _costField = new NativeArray<byte>(size, Allocator.Persistent);
            _intField = new NativeArray<ushort>(size, Allocator.Persistent);
            _openList = new NativeQueue<int3>(Allocator.Persistent);

            // cache all directions
            _directions = new NativeArray<int3>(27, Allocator.Persistent);
            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                _directions[i] = new int3(dir.x, dir.y, dir.z);
            }

            // cache directions for climbable check
            _directionsNESW = new NativeArray<int>(4, Allocator.Persistent);
            _directionsNESW[0] = 3;
            _directionsNESW[1] = 5;
            _directionsNESW[2] = 7;
            _directionsNESW[3] = 9;

            // cache block data
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
            }
        }

        /// <summary>
        /// Dispose all unmanaged memory from the AgentManager
        /// </summary>
        public void Dispose()
        {
            _transformAccess.Dispose();
            _agentDirections.Dispose();
            _agentPositions.Dispose();
            _agentSpatialMap.Dispose();
            _agentVelocity.Dispose();
            _agentActive.Dispose();


            _openList.Dispose();
            _intField.Dispose();
            _flowField.Dispose();
            _costField.Dispose();
            _blockData.Dispose();
            _directions.Dispose();
            _directionsNESW.Dispose();
        }

        /// <summary>
        /// Update each agents position in the world
        /// asynchronously using Unity jobs
        /// </summary>
        public JobHandle MoveAgentsAsync(float dt)
        {
            int3 worldSize = new int3(_world.size.x, _world.size.y, _world.size.z);
            _agentSpatialMap.Clear();

            BuildSpatialMapJob spaceJob = new BuildSpatialMapJob()
            {
                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                active = _agentActive,
                spatialMap = _agentSpatialMapWriter,
                positions = _agentPositions,
                size = spatialBucketSize
            };
            JobHandle spaceHandle = spaceJob.Schedule(_transformAccess, updateHandle);

            FlowFieldSeekJob seekJob = new FlowFieldSeekJob()
            {
                maxSpeed = maxSpeed,

                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                flowField = _flowField,
                flowDirections = _directions,
                flowFieldSize = worldSize,

                active = _agentActive,
                positions = _agentPositions,
                steering = _agentDirections,
                velocity = _agentVelocity,
            };
            JobHandle seekHandle = seekJob.Schedule(_max, 1, spaceHandle);

            AvoidCollisionBehavior avoidJob = new AvoidCollisionBehavior()
            {
                avoidForce = avoidForce,
                avoidRadius = avoidRadius,
                avoidDistance = avoidDistance,

                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                active = _agentActive,
                position = _agentPositions,
                velocity = _agentVelocity,
                steering = _agentDirections,

                spatialMap = _agentSpatialMap,
                size = spatialBucketSize
            };
            JobHandle avoidHandle = avoidJob.Schedule(_max, 1, seekHandle);

            QueueBehavior queueJob = new QueueBehavior()
            {
                maxBrakeForce = brakeForce,
                maxQueueRadius = queueRadius,
                maxQueueAhead = queueDistance,

                active = _agentActive,
                steering = _agentDirections,
                position = _agentPositions,
                velocity = _agentVelocity,

                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                size = spatialBucketSize,
                spatialMap = _agentSpatialMap
            };
            JobHandle queueHandle = queueJob.Schedule(_max, 1, avoidHandle);

            MoveAgentJob moveJob = new MoveAgentJob()
            {
                mass = mass,
                maxForce = moveForce,
                maxSpeed = maxSpeed,
                turnSpeed = turnSpeed,

                active = _agentActive,
                steering = _agentDirections,
                velocity = _agentVelocity,
                deltaTime = dt,

                world_scale = _world.scale,
                world_center = _world.data.center,
                world_offset = _world.transform.position,
                world_rotation = _world.transform.rotation,

                flowField = _flowField,
                flowFieldSize = worldSize,
            };

            return moveJob.Schedule(_transformAccess, queueHandle);
        }

        public JobHandle UpdateFlowField(Vector3Int goal, JobHandle handle)
        {
            int3 target = new int3(goal.x, goal.y, goal.z);
            int size = _world.size.x * _world.size.y * _world.size.z;
            int3 worldSize = new int3(_world.size.x, _world.size.y, _world.size.z);

            UpdateCostFieldJob costJob = new UpdateCostFieldJob()
            {
                voxels = _world.data.voxels,
                directionMask = _directionsNESW,
                directions = _directions,
                costField = _costField,
                blocks = _blockData,
                size = worldSize,
                height = height
            };
            JobHandle costHandle = costJob.Schedule(size, 1, handle);


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
                open = _openList,
                goal = target
            };
            JobHandle intHandle = intJob.Schedule(clearHandle);


            UpdateFlowFieldJob flowJob = new UpdateFlowFieldJob()
            {
                directions = _directions,
                flowField = _flowField,
                intField = _intField,
                size = worldSize,
            };

            updateHandle = flowJob.Schedule(size, 1, intHandle);
            return updateHandle;
        }

        public void TransformAccess(Transform[] transforms)
        {
            _transformAccess = new TransformAccessArray(transforms);
        }
    }
}
