﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
using VRoxel.Navigation;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationJobSpecs
{
    public class UpdateFlowFieldJobSpec
    {
        [Test]
        public void UpdatesTheFlowField()
        {
            int3 size = new int3(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(27, Allocator.Persistent);

            intField[0] = 0;
            intField[1] = 1;
            intField[2] = 2;
            intField[3] = 3;

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                directions[i] = new int3(dir.x, dir.y, dir.z);
            }

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                directions = directions,
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
            directions.Dispose();
        }

        [Test]
        public void UnFlattensIndexes()
        {
            int3 size = new int3(3, 3, 3);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(27, Allocator.Persistent);

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                directions = directions,
                flowField = flowField,
                intField = intField,
                size = size
            };

            int3 result = job.UnFlatten(0);
            int3 expected = new int3(0,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(3);
            expected = new int3(0,1,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(9);
            expected = new int3(1,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(18);
            expected = new int3(2,0,0);
            Assert.AreEqual(expected, result);

            result = job.UnFlatten(26);
            expected = new int3(2,2,2);
            Assert.AreEqual(expected, result);

            intField.Dispose();
            flowField.Dispose();
            directions.Dispose();
        }

        [Test]
        public void FlattensVector3Ints()
        {
            int3 size = new int3(3, 3, 3);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(27, Allocator.Persistent);

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                directions = directions,
                flowField = flowField,
                intField = intField,
                size = size
            };

            int3 vector = int3.zero;
            int result = job.Flatten(vector);
            int expected = 0;
            Assert.AreEqual(expected, result);

            vector = new int3(0,1,0);
            result = job.Flatten(vector);
            expected = 3;
            Assert.AreEqual(expected, result);

            vector = new int3(1,0,0);
            result = job.Flatten(vector);
            expected = 9;
            Assert.AreEqual(expected, result);

            vector = new int3(2,0,0);
            result = job.Flatten(vector);
            expected = 18;
            Assert.AreEqual(expected, result);

            vector = new int3(2,2,2);
            result = job.Flatten(vector);
            expected = 26;
            Assert.AreEqual(expected, result);

            intField.Dispose();
            flowField.Dispose();
            directions.Dispose();
        }
    }
}
