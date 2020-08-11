﻿using System.Collections;
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
    public class ResolveCollisionBehaviorSpec
    {
        [Test]
        public void CanCheckForCollisions()
        {
            float radius = 1f;
            float3 self = new float3(0, 0, 0);
            float3 target  = new float3(0, 0.5f, 0);

            ResolveCollisionBehavior job = new ResolveCollisionBehavior()
            {
                collisionRadius = radius
            };

            bool result;
            result = job.Collision(self, target, radius);
            Assert.AreEqual(true, result);      // inside

            target += new float3(0, 0.5f, 0);
            result = job.Collision(self, target, radius);
            Assert.AreEqual(true, result);      // on the edge

            target += new float3(0, 0.5f, 0);
            result = job.Collision(self, target, radius);
            Assert.AreEqual(false, result);     // outside
        }

        [Test]
        public void CanApplyCollisionForce()
        {
            NativeArray<float3> velocity = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<float3> position = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            ResolveCollisionBehavior job = new ResolveCollisionBehavior()
            {
                collisionRadius = 1f,
                collisionForce = 0.5f,
                steering = steering,
                position = position,
                velocity = velocity
            };

            float3 self = float3.zero;
            float3 target = float3.zero;
            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(float3.zero, steering[0]);

            steering[0] = float3.zero;

            target = new float3(0,1,0);
            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(new float3(0, -0.5f, 0), steering[0]);

            steering.Dispose();
            position.Dispose();
            velocity.Dispose();
        }
    }
}