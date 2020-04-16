using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;

using VRoxel.Navigation;

namespace NavigationJobSpecs
{
    public class LocalAvoidanceJobSpec
    {
        [Test]
        public void CanGetTheCurrentSpatialBucket()
        {
            LocalAvoidanceJob job = new LocalAvoidanceJob()
            {
                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,
                size = new int3(2,2,2),
            };

            Assert.AreEqual(int3.zero, job.GetSpatialBucket(float3.zero));
            Assert.AreEqual(int3.zero, job.GetSpatialBucket(new float3(0,1,0)));
            Assert.AreEqual(new int3(0,1,0), job.GetSpatialBucket(new float3(0,2,0)));
            Assert.AreEqual(new int3(0,2,0), job.GetSpatialBucket(new float3(0,4,0)));
        }
    }
}
