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
    public class GravityBehaviorSpec
    {
        [Test]
        public void AgentsFallWhenInAir()
        {
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ velocity = new float3(0,0,0) };

            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            steering[0] = new float3(0,0,0);

            NativeArray<bool> active = new NativeArray<bool>(1, Allocator.Persistent);
            active[0] = true;

            NativeArray<VRoxel.Navigation.Block> blocks = new NativeArray<VRoxel.Navigation.Block>(1, Allocator.Persistent);
            VRoxel.Navigation.Block airBlock = new VRoxel.Navigation.Block();
            airBlock.solid = false; airBlock.cost = 1; blocks[0] = airBlock;

            NativeArray<byte> voxels = new NativeArray<byte>(1, Allocator.Persistent);

            AgentWorld world = new AgentWorld()
            {
                scale = 1f,
                offset = float3.zero,
                center = new float3(0.5f, 0.5f, 0.5f),
                rotation = quaternion.identity,
            };

            GravityBehavior job = new GravityBehavior()
            {
                gravity = new float3(0,-9f,0),
                world = world,
                steering = steering,
                agents = agents,
                active = active,
                blocks = blocks,
                voxels = voxels,
            };

            job.Schedule(1,1).Complete();

            Assert.AreEqual(new float3(0,-9,0), steering[0]);

            agents.Dispose();
            active.Dispose();
            blocks.Dispose();
            voxels.Dispose();
            steering.Dispose();
        }

        [Test]
        public void AgentsRiseWhenBuried()
        {
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ velocity = new float3(0,0,0) };

            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            steering[0] = new float3(0,0,0);

            NativeArray<bool> active = new NativeArray<bool>(1, Allocator.Persistent);
            active[0] = true;

            NativeArray<VRoxel.Navigation.Block> blocks = new NativeArray<VRoxel.Navigation.Block>(1, Allocator.Persistent);
            VRoxel.Navigation.Block solidBlock = new VRoxel.Navigation.Block();
            solidBlock.solid = true; solidBlock.cost = 1; blocks[0] = solidBlock;

            NativeArray<byte> voxels = new NativeArray<byte>(1, Allocator.Persistent);

            AgentWorld world = new AgentWorld()
            {
                scale = 1f,
                offset = float3.zero,
                center = new float3(0.5f, 0.5f, 0.5f),
                rotation = quaternion.identity,
            };

            GravityBehavior job = new GravityBehavior()
            {
                gravity = new float3(0,-4f,0),
                world = world,
                steering = steering,
                agents = agents,
                active = active,
                blocks = blocks,
                voxels = voxels,
            };

            job.Schedule(1,1).Complete();

            Assert.AreEqual(new float3(0,2,0), steering[0]);

            agents.Dispose();
            active.Dispose();
            blocks.Dispose();
            voxels.Dispose();
            steering.Dispose();
        }
    }
}