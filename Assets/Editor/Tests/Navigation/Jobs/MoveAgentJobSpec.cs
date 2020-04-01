using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Navigation;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationSpecs
{
    public class MoveAgentJobSpec
    {
        [Test]
        public void UpdatesPositionAndRotation()
        {
            Transform[] transforms = new Transform[2];
            transforms[0] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            transforms[1] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            TransformAccessArray asyncTransforms = new TransformAccessArray(transforms);

            NativeArray<Vector3> directions = new NativeArray<Vector3>(2, Allocator.Persistent);
            directions[0] = Vector3.right;
            directions[1] = Vector3.left;

            float speed = 1f;
            MoveAgentJob job = new MoveAgentJob()
            {
                speed = speed,
                deltaTime = Time.deltaTime,
                directions = directions
            };

            Vector3 position_0 = transforms[0].position + (directions[0] * speed * Time.deltaTime);
            Vector3 position_1 = transforms[1].position + (directions[1] * speed * Time.deltaTime);
            Quaternion rotation_0 = Quaternion.LookRotation(directions[0], Vector3.up);
            Quaternion rotation_1 = Quaternion.LookRotation(directions[1], Vector3.up);

            JobHandle handle = job.Schedule(asyncTransforms);
            handle.Complete();

            Assert.AreEqual(position_0, transforms[0].position);
            Assert.AreEqual(position_1, transforms[1].position);
            Assert.AreEqual(rotation_0, transforms[0].rotation);
            Assert.AreEqual(rotation_1, transforms[1].rotation);

            directions.Dispose();
            asyncTransforms.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}
