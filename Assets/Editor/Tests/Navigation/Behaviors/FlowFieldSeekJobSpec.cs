﻿using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
using VRoxel.Navigation;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationBehaviorSpecs
{
    public class FlowFieldSeekJobSpec
    {
        [Test]
        public void UpdatesDirections()
        {
            NativeArray<float3> velocity = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<float3> directions = new NativeArray<float3>(1, Allocator.Persistent);
            NativeArray<float3> positions = new NativeArray<float3>(1, Allocator.Persistent);
            positions[0] = Vector3.up;

            NativeArray<byte> flowField = new NativeArray<byte>(1, Allocator.Persistent);
            NativeArray<int3> flowDirections = new NativeArray<int3>(27, Allocator.Persistent);

            for (int i = 0; i < 1; i++)
                flowField[i] = (byte)Direction3Int.Name.Up;

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                flowDirections[i] = new int3(dir.x, dir.y, dir.z);
            }

            FlowFieldSeekJob job = new FlowFieldSeekJob()
            {
                maxSpeed = 1f,

                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,

                flowField = flowField,
                flowDirections = flowDirections,
                flowFieldSize = new int3(1,1,1),

                positions = positions,
                steering = directions,
                velocity = velocity
            };

            JobHandle handle = job.Schedule(1,1);
            handle.Complete();

            Assert.AreEqual(new float3(0,1,0), directions[0]);

            flowDirections.Dispose();
            flowField.Dispose();
            positions.Dispose();
            directions.Dispose();
            velocity.Dispose();
        }
    }
}