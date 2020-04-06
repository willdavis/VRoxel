using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Core;
using VRoxel.Navigation;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationSpecs
{
    public class UpdateFlowFieldJobSpec
    {
        [Test]
        public void UpdatesTheFlowField()
        {
            Vector3Int size = new Vector3Int(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<int> intField = new NativeArray<int>(flatSize, Allocator.Persistent);
            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            intField[0] = 0;
            intField[1] = 1;
            intField[2] = 2;
            intField[3] = 3;

            for (int i = 0; i < 27; i++)
                flowDirections[i] = Direction3Int.Directions[i];

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                flowDirections = flowDirections,
                flowField = flowField,
                intField = intField,
                size = size
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            Assert.AreEqual((byte)Direction3Int.Name.Zero,      flowField[0]);
            Assert.AreEqual((byte)Direction3Int.Name.South,     flowField[1]);
            Assert.AreEqual((byte)Direction3Int.Name.Down,      flowField[2]);
            Assert.AreEqual((byte)Direction3Int.Name.DownSouth, flowField[3]);

            intField.Dispose();
            flowField.Dispose();
            flowDirections.Dispose();
        }

        [Test]
        public void UnFlattensIndexes()
        {
            Vector3Int size = new Vector3Int(3, 3, 3);
            int flatSize = size.x * size.y * size.z;

            NativeArray<int> intField = new NativeArray<int>(flatSize, Allocator.Persistent);
            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                flowDirections = flowDirections,
                flowField = flowField,
                intField = intField,
                size = size
            };

            Vector3Int result = job.UnFlatten(0);
            Vector3Int expected = new Vector3Int(0,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(3);
            expected = new Vector3Int(0,1,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(9);
            expected = new Vector3Int(1,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(18);
            expected = new Vector3Int(2,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(26);
            expected = new Vector3Int(2,2,2);
            Assert.AreEqual(expected, result);

            intField.Dispose();
            flowField.Dispose();
            flowDirections.Dispose();
        }

        [Test]
        public void FlattensVector3Ints()
        {
            Vector3Int size = new Vector3Int(3, 3, 3);
            int flatSize = size.x * size.y * size.z;

            NativeArray<int> intField = new NativeArray<int>(flatSize, Allocator.Persistent);
            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                flowDirections = flowDirections,
                flowField = flowField,
                intField = intField,
                size = size
            };

            Vector3Int vector = Vector3Int.zero;
            int result = job.Flatten(vector);
            int expected = 0;
            Assert.AreEqual(expected, result);

            vector = new Vector3Int(0,1,0);
            result = job.Flatten(vector);
            expected = 3;
            Assert.AreEqual(expected, result);

            vector = new Vector3Int(1,0,0);
            result = job.Flatten(vector);
            expected = 9;
            Assert.AreEqual(expected, result);

            vector = new Vector3Int(2,0,0);
            result = job.Flatten(vector);
            expected = 18;
            Assert.AreEqual(expected, result);

            vector = new Vector3Int(2,2,2);
            result = job.Flatten(vector);
            expected = 26;
            Assert.AreEqual(expected, result);

            intField.Dispose();
            flowField.Dispose();
            flowDirections.Dispose();
        }
    }
}
