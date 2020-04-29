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
            Assert.AreEqual(true, result);

            best = new float3(0, 0.5f, 0);
            target  = new float3(0, 1f, 0);
            result = job.MostThreatening(0, target, best);
            Assert.AreEqual(false, result);

            positions.Dispose();
        }

        /*
        [Test]
        public void CanApplyAvoidanceForce()
        {
            
        }

        [Test]
        public void CanDetectAnObstruction()
        {
            
        }

        [Test]
        public void AvoidsNearbyAgents()
        {
            
        }
        */
    }
}
