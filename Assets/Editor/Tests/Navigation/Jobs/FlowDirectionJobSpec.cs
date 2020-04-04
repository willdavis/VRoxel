using UnityEngine;
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

            NativeArray<byte> flowField = new NativeArray<byte>(1, Allocator.Persistent);
            NativeArray<Vector3Int> flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            FlowDirectionJob job = new FlowDirectionJob()
            {
                world_scale = 1f,
                world_offset = Vector3.zero,
                world_center = new Vector3(0.5f, 0.5f, 0.5f),
                world_rotation = Quaternion.identity,

                flowField = flowField,
                flowDirections = flowDirections,

                directions = directions
            };

            JobHandle handle = job.Schedule(transformAccess);
            handle.Complete();

            Assert.AreEqual(Vector3.up, directions[0]);
            Assert.AreEqual(Vector3.up, directions[1]);

            flowDirections.Dispose();
            flowField.Dispose();

            directions.Dispose();
            transformAccess.Dispose();
            foreach (var t in transforms)
                GameObject.DestroyImmediate(t.gameObject);
        }
    }
}