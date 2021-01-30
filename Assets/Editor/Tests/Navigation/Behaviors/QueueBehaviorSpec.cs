using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Navigation;
using VRoxel.Navigation.Agents;

namespace NavigationBehaviorSpecs
{
    public class QueueBehaviorSpec
    {
        [Test]
        public void ApplyBrakeForceToAgents()
        {
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ velocity = new float3(0,1,0) };

            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            steering[0] = new float3(1,0,0);

            float maxBrakeForce = 0.8f;
            QueueBehavior job = new QueueBehavior()
            {
                maxBrakeForce = maxBrakeForce,
                steering = steering,
                agents = agents
            };

            float3 expectedVelocity = agents[0].velocity;
            float3 expectedSteering = steering[0];

            expectedSteering += -expectedSteering * maxBrakeForce;
            expectedSteering += -agents[0].velocity;

            job.ApplyBrakeForce(0);

            Assert.AreEqual(expectedVelocity, agents[0].velocity);  // velocity should not have changed
            Assert.AreEqual(expectedSteering, steering[0]);     // steering should have changed

            steering.Dispose();
            agents.Dispose();
        }

        [Test]
        public void DetectCollisionsWithAgents()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0, 0.5f, 0);

            NativeMultiHashMap<int3, SpatialMapData> spatialMap = new NativeMultiHashMap<int3, SpatialMapData>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, new SpatialMapData() { position = position1 });
            spatialMap.Add(int3.zero, new SpatialMapData() { position = position2 });

            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(2, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ position = position1 };
            agents[1] = new AgentKinematics(){ position = position2 };

            QueueBehavior job = new QueueBehavior()
            {
                spatialMap = spatialMap,
                maxQueueRadius = 0.25f,
                agents = agents,
                maxDepth = 100,
            };

            bool collision;
            collision = job.DetectCollision(0, float3.zero, int3.zero);
            Assert.AreEqual(false, collision);

            collision = job.DetectCollision(0, new float3(0, 0.25f, 0), int3.zero);
            Assert.AreEqual(true, collision);

            spatialMap.Dispose();
            agents.Dispose();
        }

        [Test]
        public void AddBrakeForceWhenAgentsCollide()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0f, 1f, 0f);

            NativeMultiHashMap<int3, SpatialMapData> spatialMap = new NativeMultiHashMap<int3, SpatialMapData>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, new SpatialMapData() { position = position1 });
            spatialMap.Add(int3.zero, new SpatialMapData() { position = position2 });

            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(2, Allocator.Persistent);
            agents[0] = new AgentKinematics(){ position = position1, velocity = new float3(0, 0, 0) };
            agents[1] = new AgentKinematics(){ position = position2, velocity = new float3(0, 1, 0) };

            NativeArray<float3> steering = new NativeArray<float3>(2, Allocator.Persistent);
            steering[0] = new float3(0, 1, 0);
            steering[1] = new float3(1, 0, 1);

            NativeArray<AgentBehaviors> active = new NativeArray<AgentBehaviors>(2, Allocator.Persistent);
            for (int i = 0; i < 2; i++) { active[i] = AgentBehaviors.Queueing; }

            AgentWorld world = new AgentWorld()
            {
                scale = 1f,
                offset = float3.zero,
                center = new float3(0.5f, 0.5f, 0.5f),
                rotation = quaternion.identity,
            };

            QueueBehavior job = new QueueBehavior()
            {
                maxBrakeForce = 0.8f,
                maxQueueAhead = 1f,
                maxQueueRadius = 1f,
                maxDepth = 100,

                world = world,
                size = new int3(1,1,1),
                spatialMap = spatialMap,
                steering = steering,
                behaviors = active,
                agents = agents
            };

            job.Schedule(2,1).Complete();

            // first agent should be braking
            float3 expected = new float3(0, 0.2f, 0);
            Assert.AreEqual(true, Mathf.Approximately(expected.x, steering[0].x));
            Assert.AreEqual(true, Mathf.Approximately(expected.y, steering[0].y));
            Assert.AreEqual(true, Mathf.Approximately(expected.z, steering[0].z));

            // second agent should not be braking
            expected = new float3(1, 0, 1);
            Assert.AreEqual(true, Mathf.Approximately(expected.x, steering[1].x));
            Assert.AreEqual(true, Mathf.Approximately(expected.y, steering[1].y));
            Assert.AreEqual(true, Mathf.Approximately(expected.z, steering[1].z));

            spatialMap.Dispose();
            steering.Dispose();
            agents.Dispose();
            active.Dispose();
        }
    }
}
