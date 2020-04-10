using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
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
            Transform[] transforms = new Transform[1];
            transforms[0] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            transforms[0].position += Vector3.up;

            TransformAccessArray transformAccess = new TransformAccessArray(transforms);
            NativeArray<float3> directions = new NativeArray<float3>(1, Allocator.Persistent);

            NativeArray<byte> flowField = new NativeArray<byte>(1, Allocator.Persistent);
            NativeArray<int3> flowDirections = new NativeArray<int3>(27, Allocator.Persistent);

            for (int i = 0; i < 1; i++)
                flowField[i] = (byte)Direction3Int.Name.Up;

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                flowDirections[i] = new int3(dir.x, dir.y, dir.z);
            }

            FlowDirectionJob job = new FlowDirectionJob()
            {
                world_scale = 1f,
                world_offset = float3.zero,
                world_center = new float3(0.5f, 0.5f, 0.5f),
                world_rotation = quaternion.identity,

                flowField = flowField,
                flowDirections = flowDirections,
                flowFieldSize = new int3(1,1,1),

                directions = directions
            };

            JobHandle handle = job.Schedule(transformAccess);
            handle.Complete();

            Assert.AreEqual(new float3(0,1,0), directions[0]);

            flowDirections.Dispose();
            flowField.Dispose();

            directions.Dispose();
            transformAccess.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}