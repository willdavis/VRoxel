using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
using VRoxel.Navigation;
using VRoxel.Navigation.Agents;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationBehaviorSpecs
{
    public class FlowFieldSeekJobSpec
    {
        [Test]
        public void UpdatesDirections()
        {
            NativeArray<float3> directions = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ position = Vector3.up };

            NativeArray<bool> active = new NativeArray<bool>(1, Allocator.Persistent);
            active[0] = true;

            NativeArray<byte> flowField = new NativeArray<byte>(1, Allocator.Persistent);
            NativeArray<int3> flowDirections = new NativeArray<int3>(27, Allocator.Persistent);

            for (int i = 0; i < 1; i++)
                flowField[i] = (byte)Direction3Int.Name.Up;

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                flowDirections[i] = new int3(dir.x, dir.y, dir.z);
            }

            NativeArray<int> agentMovementTypes = new NativeArray<int>(1, Allocator.Persistent);
            NativeArray<AgentMovement> movementTypes = new NativeArray<AgentMovement>(1, Allocator.Persistent);
            movementTypes[0] = new AgentMovement() { mass = 1f, topSpeed = 1f, turnSpeed = 1f };

            FlowFieldSeekJob job = new FlowFieldSeekJob()
            {
                movementTypes = movementTypes,
                agentMovement = agentMovementTypes,

                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,

                flowField = flowField,
                flowDirections = flowDirections,
                flowFieldSize = new int3(1,1,1),

                active = active,
                agents = agents,
                steering = directions,
            };

            JobHandle handle = job.Schedule(1,1);
            handle.Complete();

            Assert.AreEqual(new float3(0,1,0), directions[0]);

            flowDirections.Dispose();
            flowField.Dispose();
            directions.Dispose();
            agents.Dispose();
            active.Dispose();
            movementTypes.Dispose();
            agentMovementTypes.Dispose();
        }
    }
}