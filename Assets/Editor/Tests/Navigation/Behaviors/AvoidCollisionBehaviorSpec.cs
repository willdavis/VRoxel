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
    public class AvoidCollisionBehaviorSpec
    {
        [Test]
        public void CanCheckIfVectorIntersectsCircle()
        {
            float radius = 1f;
            float3 center = new float3(0, 0, 0);
            float3 ahead  = new float3(0, 1, 0);
            float3 ahead2 = ahead * 0.5f;

            AvoidCollisionBehavior job = new AvoidCollisionBehavior() {  };

            bool result;
            result = job.IntersectsCircle(ahead, ahead2, center, radius);
            Assert.AreEqual(true, result);      // inside

            ahead += new float3(0,1f,0);
            ahead2 = ahead * 0.5f;

            result = job.IntersectsCircle(ahead, ahead2, center, radius);
            Assert.AreEqual(true, result);      // on the edge

            ahead += new float3(0,1f,0);
            ahead2 = ahead * 0.5f;

            result = job.IntersectsCircle(ahead, ahead2, center, radius);
            Assert.AreEqual(false, result);     // outside
        }

        [Test]
        public void CanCheckForMostThreateningAgent()
        {
            bool result;
            float3 target, best;
            NativeArray<float3> positions = new NativeArray<float3>(1, Allocator.Persistent);
            AvoidCollisionBehavior job = new AvoidCollisionBehavior()
            {
                position = positions
            };

            best = new float3(0, 1f, 0);
            target  = new float3(0, 0.5f, 0);
            result = job.MostThreatening(0, target, best);
            Assert.AreEqual(true, result);      // more threatening

            best = new float3(0, 0.5f, 0);
            target  = new float3(0, 1f, 0);
            result = job.MostThreatening(0, target, best);
            Assert.AreEqual(false, result);     // less threatening

            positions.Dispose();
        }

        [Test]
        public void CanApplyAvoidanceForce()
        {
            float3 ahead, center;
            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            AvoidCollisionBehavior job = new AvoidCollisionBehavior()
            {
                maxAvoidForce = 0.5f,
                steering = steering
            };

            center = float3.zero;
            ahead = float3.zero;
            job.ApplyAvoidanceForce(0, ahead, center);
            Assert.AreEqual(float3.zero, steering[0]);

            steering[0] = float3.zero;

            ahead = new float3(0,1,0);
            job.ApplyAvoidanceForce(0, ahead, center);
            Assert.AreEqual(new float3(0, 0.5f, 0), steering[0]);

            steering.Dispose();
        }

        [Test]
        public void CanDetectAnObstruction()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0f, 1f, 0f);
            float3 max = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

            NativeMultiHashMap<int3, float3> spatialMap = new NativeMultiHashMap<int3, float3>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, position1);
            spatialMap.Add(int3.zero, position2);

            NativeArray<float3> positions = new NativeArray<float3>(2, Allocator.Persistent);
            positions[0] = position1;
            positions[1] = position2;

            AvoidCollisionBehavior job = new AvoidCollisionBehavior()
            {
                maxAvoidRadius = 0.5f,
                position = positions,
                spatialMap = spatialMap
            };

            // when ahead vector is in range
            bool obstructed;
            float3 ahead = new float3(0, 1f, 0);
            float3 ahead2 = new float3(0, 0.5f, 0);
            float3 closest = max;

            obstructed = job.DetectObstruction(0, ahead, ahead2, int3.zero, ref closest);
            Assert.AreEqual(true, obstructed);
            Assert.AreEqual(position2, closest);

            // when ahead2 vector is in range
            ahead = new float3(0, 2, 0);
            ahead2 = new float3(0, 1, 0);
            closest = max;

            obstructed = job.DetectObstruction(0, ahead, ahead2, int3.zero, ref closest);
            Assert.AreEqual(true, obstructed);
            Assert.AreEqual(position2, closest);

            // when not obstructed
            ahead = new float3(0, 0.25f, 0);
            ahead2 = new float3(0, 0.125f, 0);
            closest = max;

            obstructed = job.DetectObstruction(0, ahead, ahead2, int3.zero, ref closest);
            Assert.AreEqual(false, obstructed);
            Assert.AreEqual(max, closest);

            spatialMap.Dispose();
            positions.Dispose();
        }

        [Test]
        public void AvoidsNearbyAgents()
        {
            float3 position1 = float3.zero;
            float3 position2 = new float3(0f, 1f, 0f);
            float3 max = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

            NativeMultiHashMap<int3, float3> spatialMap = new NativeMultiHashMap<int3, float3>(1, Allocator.Persistent);
            spatialMap.Add(int3.zero, position1);
            spatialMap.Add(int3.zero, position2);

            NativeArray<float3> positions = new NativeArray<float3>(2, Allocator.Persistent);
            positions[0] = position1;
            positions[1] = position2;

            NativeArray<float3> steering = new NativeArray<float3>(2, Allocator.Persistent);
            NativeArray<float3> velocity = new NativeArray<float3>(2, Allocator.Persistent);
            velocity[0] = new float3(0, 0, 0);
            velocity[1] = new float3(0, 1, 0);


            AvoidCollisionBehavior job = new AvoidCollisionBehavior()
            {
                maxAvoidLength = 1f,
                maxAvoidRadius = 1f,
                maxAvoidForce = 1f,

                size = new int3(1,1,1),
                spatialMap = spatialMap,
                steering = steering,
                position = positions,
                velocity = velocity,

                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,
            };

            job.Schedule(2, 1).Complete();

            Assert.AreEqual(new float3(0, -1, 0), steering[0]);
            Assert.AreEqual(new float3(0,  0, 0), steering[1]);

            spatialMap.Dispose();
            positions.Dispose();
            steering.Dispose();
            velocity.Dispose();
        }
    }
}
