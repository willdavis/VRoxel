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
    public class CollisionBehaviorSpec
    {
        [Test]
        public void CanCheckForCollisions()
        {
            int height = 1;
            float radius = 1f;
            float3 start = new float3(0.25f, 0, 0);
            AgentKinematics self = new AgentKinematics()
                { position = float3.zero };
            SpatialMapData target = new SpatialMapData()
                { position = start, height = height };

            CollisionBehavior job = new CollisionBehavior()
            {
                minDistance = 0.01f,
                collision = new AgentCollision()
                    { radius = radius, height = height }
            };

            /// test 2D circle intersection

            bool result;
            result = job.Collision(self, target);
            Assert.AreEqual(true, result);      // inside

            target.position += new float3(0.25f, 0, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(true, result);      // on the edge

            target.position += new float3(1f, 0, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(false, result);     // outside

            /// test height intersection

            target.position = new float3(0, 0.5f, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(true, result);      // inside (collision)

            target.position = new float3(0, 1f, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(false, result);      // above (no collision)

            target.position = new float3(0, -1f, 0);
            result = job.Collision(self, target);
            Assert.AreEqual(false, result);      // below (no collision)
        }

        [Test]
        public void CanApplyCollisionForce()
        {
            NativeArray<AgentKinematics> agents = new NativeArray<AgentKinematics>(1, Allocator.Persistent);
            NativeArray<float3> steering = new NativeArray<float3>(1, Allocator.Persistent);

            NativeArray<int> agentMovementTypes = new NativeArray<int>(1, Allocator.Persistent);
            NativeArray<AgentMovement> movementTypes = new NativeArray<AgentMovement>(1, Allocator.Persistent);
            movementTypes[0] = new AgentMovement() { mass = 1f, topSpeed = 1f, turnSpeed = 1f };

            int height = 1;

            CollisionBehavior job = new CollisionBehavior()
            {
                minDistance = 0.01f,
                movement = agentMovementTypes,
                movementConfigs = movementTypes,
                collision = new AgentCollision()
                    { radius = 0.5f, height = height },
                steering = steering,
                agents = agents,
            };

            /// test no steering forces are applied
            /// when the target and source are the same

            SpatialMapData target = new SpatialMapData()
                { position = float3.zero, height = height };

            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(float3.zero, steering[0]);
            steering[0] = float3.zero;

            /// test steering forces can be applied
            /// when the agents are within collision distance

            target = new SpatialMapData()
            {
                mass = 1f, radius = 0.5f,
                position = new float3(1f, 0, 0),
                height = height
            };

            job.ApplyCollisionForce(0, target);
            Assert.AreEqual(new float3(-1f, 0, 0), steering[0]);

            agents.Dispose();
            steering.Dispose();
        }
    }
}
