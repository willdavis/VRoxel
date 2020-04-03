﻿using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Navigation;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationSpecs
{
    public class FlowDirectionJobSpec
    {
        [Test]
        public void UpdatesDirections()
        {
            int size = 2;
            Transform[] transforms = new Transform[2];
            transforms[0] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            transforms[1] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            TransformAccessArray transformAccess = new TransformAccessArray(transforms);
            NativeArray<Vector3> directions = new NativeArray<Vector3>(size, Allocator.Persistent);

            FlowDirectionJob job = new FlowDirectionJob()
            {
                directions = directions
            };

            JobHandle handle = job.Schedule(transformAccess);
            handle.Complete();

            Assert.AreEqual(Vector3.up, directions[0]);
            Assert.AreEqual(Vector3.up, directions[1]);

            directions.Dispose();
            transformAccess.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}