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
    public class ResolveCollisionBehaviorSpec
    {
        [Test]
        public void CanCheckForCollisions()
        {
            float radius = 1f;
            AgentKinematics self = new AgentKinematics() { position = float3.zero };
            SpatialMapData target = new SpatialMapData() { position = new float3(0, 0.5f, 0) };

            ResolveCollisionBehavior job = new ResolveCollisionBehavior()
            {
                collision = new AgentCollision() { radius = radius }
            };

            bool result;
            result = job.Collision(self, target);
            Assert.AreEqual(true, result);      // inside

            target.position += new float3(0, 0.5f, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(true, result);      // on the edge

            target.position += new float3(0, 0.5f, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(false, result);     // outside
        }

        [Test]
        public void CanApplyCollisionForce()
        {
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);
            ResolveCollisionBehavior job = new ResolveCollisionBehavior()
            {
                collision = new AgentCollision() { radius = 1f },
                collisionForce = 0.5f,
                steering = steering,
                agents = agents,
            };

            SpatialMapData target = new SpatialMapData()
                { position = float3.zero };

            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(float3.zero, steering[0]);

            steering[0] = float3.zero;

            target = new SpatialMapData() { position = new float3(0, 1, 0) };
            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(new float3(0, -0.5f, 0), steering[0]);

            agents.Dispose();
            steering.Dispose();
        }
    }
}
