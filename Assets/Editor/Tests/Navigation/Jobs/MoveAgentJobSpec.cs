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
        public void CanClampVectors()
        {
            MoveAgentJob job = new MoveAgentJob(){};

            // ignores vectors less than the limit
            Assert.AreEqual(
                new float3(1,1,1),
                job.Clamp(new float3(1,1,1), 10f)
            );

            // scales vectors if they are larger than the limit
            Assert.AreEqual(
                new float3(0,1,0),
                job.Clamp(new float3(0,2,0), 1f)
            );

            Assert.AreEqual(
                float3.zero,
                job.Clamp(new float3(1,1,1), 0f)
            );
        }


        [Test]
        public void UpdatesPositionAndRotation()
        {
            Transform[] transforms = new Transform[2];
            transforms[0] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            transforms[1] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            TransformAccessArray asyncTransforms = new TransformAccessArray(transforms);

            NativeArray<float3> velocity = new NativeArray<float3>(2, Allocator.Persistent);
            NativeArray<float3> directions = new NativeArray<float3>(2, Allocator.Persistent);
            directions[0] = Vector3.right;
            directions[1] = Vector3.left;

            NativeArray<byte> flowField = new NativeArray<byte>(1, Allocator.Persistent);
            for (int i = 0; i < 1; i++)
                flowField[i] = 1;

            MoveAgentJob job = new MoveAgentJob()
            {
                mass = 1f,
                maxForce = 1f,
                maxSpeed = 1f,
                turnSpeed = 1f,

                steering = directions,
                velocity = velocity,
                deltaTime = Time.deltaTime,

                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,

                flowField = flowField,
                flowFieldSize = new int3(1,1,1)
            };

            Vector3 position_0 = transforms[0].position;
            Vector3 position_1 = transforms[1].position;

            Quaternion rotation_0 = transforms[0].rotation;
            Quaternion rotation_1 = transforms[1].rotation;

            JobHandle handle = job.Schedule(asyncTransforms);
            handle.Complete();

            Assert.AreNotEqual(position_0, transforms[0].position);
            Assert.AreNotEqual(position_1, transforms[1].position);

            Assert.AreNotEqual(rotation_0, transforms[0].rotation);
            Assert.AreNotEqual(rotation_1, transforms[1].rotation);


            flowField.Dispose();
            velocity.Dispose();
            directions.Dispose();
            asyncTransforms.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}
