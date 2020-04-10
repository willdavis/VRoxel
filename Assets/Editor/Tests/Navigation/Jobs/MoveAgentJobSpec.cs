using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

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

            NativeArray<float3> directions = new NativeArray<float3>(2, Allocator.Persistent);
            directions[0] = Vector3.right;
            directions[1] = Vector3.left;

            float speed = 1f;
            MoveAgentJob job = new MoveAgentJob()
            {
                speed = speed,
                deltaTime = Time.deltaTime,
                directions = directions
            };

            float3 position_0 = new float3(
                transforms[0].position.x, transforms[0].position.y, transforms[0].position.z
            );
            float3 position_1 = new float3(
                transforms[1].position.x, transforms[1].position.y, transforms[1].position.z
            );

            float3 expected_position_0 = position_0 + (directions[0] * speed * Time.deltaTime);
            float3 expected_position_1 = position_1 + (directions[1] * speed * Time.deltaTime);
            quaternion rotation_0 = quaternion.LookRotation(directions[0], new float3(0,1,0));
            quaternion rotation_1 = quaternion.LookRotation(directions[1], new float3(0,1,0));

            JobHandle handle = job.Schedule(asyncTransforms);
            handle.Complete();

            // check that the two vectors have the same position
            float3 position = new float3(
                transforms[0].position.x,
                transforms[0].position.y,
                transforms[0].position.z
            );
            Assert.AreEqual(expected_position_0, position);

            position = new float3(
                transforms[1].position.x,
                transforms[1].position.y,
                transforms[1].position.z
            );
            Assert.AreEqual(expected_position_1, position);

            // check that the two quaternions have the same orientation
            quaternion rotation = transforms[0].rotation;
            Assert.AreEqual(math.abs(math.dot(rotation_0, rotation)), 1);

            rotation = transforms[1].rotation;
            Assert.AreEqual(math.abs(math.dot(rotation_1, rotation)), 1);

            directions.Dispose();
            asyncTransforms.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}
