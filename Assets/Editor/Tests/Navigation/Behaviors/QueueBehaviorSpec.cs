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

namespace NavigationBehaviorSpecs
{
    public class QueueBehaviorSpec
    {
        [Test]
        public void ApplyBrakeForceToAgents()
        {
            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<float3> velocity = new NativeArray<float3>(1, Allocator.Persistent);
            float maxBrakeForce = 0.8f;

            steering[0] = new float3(1,0,0);
            velocity[0] = new float3(0,1,0);

            QueueBehavior job = new QueueBehavior()
            {
                maxBrakeForce = maxBrakeForce,
                steering = steering,
                velocity = velocity
            };

            float3 expectedVelocity = velocity[0];
            float3 expectedSteering = steering[0];

            expectedSteering += -expectedSteering * maxBrakeForce;
            expectedSteering += - velocity[0];

            job.ApplyBrakeForce(0);

            Assert.AreEqual(expectedVelocity, velocity[0]);     // velocity should not have changed
            Assert.AreEqual(expectedSteering, steering[0]);     // steering should have changed

            steering.Dispose();
            velocity.Dispose();
        }

        [Test]
        public void DetectCollisionsWithAgents()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0, 0.5f, 0);

            NativeMultiHashMap<int3, float3> spatialMap = new NativeMultiHashMap<int3, float3>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, position1);
            spatialMap.Add(int3.zero, position2);

            NativeArray<float3> positions = new NativeArray<float3>(2, Allocator.Persistent);
            positions[0] = position1;
            positions[1] = position2;

            QueueBehavior job = new QueueBehavior()
            {
                spatialMap = spatialMap,
                maxQueueRadius = 0.25f,
                position = positions
            };

            bool collision;
            collision = job.DetectCollision(0, float3.zero, int3.zero);
            Assert.AreEqual(false, collision);

            collision = job.DetectCollision(0, new float3(0, 0.25f, 0), int3.zero);
            Assert.AreEqual(true, collision);

            spatialMap.Dispose();
            positions.Dispose();
        }

        [Test]
        public void AddBrakeForceWhenAgentsCollide()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0f, 1f, 0f);

            NativeMultiHashMap<int3, float3> spatialMap = new NativeMultiHashMap<int3, float3>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, position1);
            spatialMap.Add(int3.zero, position2);

            NativeArray<float3> positions = new NativeArray<float3>(2, Allocator.Persistent);
            positions[0] = position1;
            positions[1] = position2;

            NativeArray<float3> steering = new NativeArray<float3>(2, Allocator.Persistent);
            steering[0] = new float3(0, 1, 0);
            steering[1] = new float3(1, 0, 1);

            NativeArray<float3> velocity = new NativeArray<float3>(2, Allocator.Persistent);
            velocity[0] = new float3(0, 0, 0);
            velocity[1] = new float3(0, 1, 0);

            QueueBehavior job = new QueueBehavior()
            {
                maxBrakeForce = 0.8f,
                maxQueueAhead = 1f,
                maxQueueRadius = 1f,

                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,

                size = new int3(1,1,1),
                spatialMap = spatialMap,
                position = positions,
                steering = steering,
                velocity = velocity
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
            positions.Dispose();
            steering.Dispose();
            velocity.Dispose();
        }
    }
}
