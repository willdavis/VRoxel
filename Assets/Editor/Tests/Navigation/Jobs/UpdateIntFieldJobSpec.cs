using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Navigation;
using VRoxel.Core;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationJobSpecs
{
    public class UpdateIntFieldJobSpec
    {
        [Test]
        public void UpdatesTheIntegrationField()
        {
            int3 size = new int3(1, 1, 4);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> costField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(27, Allocator.Persistent);
            NativeList<int3> targets = new NativeList<int3>(Allocator.Persistent);
            NativeQueue<int3> open = new NativeQueue<int3>(Allocator.Persistent);

            targets.Add(int3.zero);

            for (int i = 0; i < flatSize; i++)
            {
                costField[i] = 2;
                intField[i] = ushort.MaxValue;
            }

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                directions[i] = new int3(dir.x, dir.y, dir.z);
            }

            UpdateIntFieldJob job = new UpdateIntFieldJob()
            {
                size = size,
                open = open,
                minCostDiff = 10,
                targets = targets,
                directions = directions,
                costField = costField,
                intField = intField
            };

            JobHandle handle = job.Schedule();
            handle.Complete();

            Assert.AreEqual(0, intField[0]);
            Assert.AreEqual(2, intField[1]);
            Assert.AreEqual(4, intField[2]);
            Assert.AreEqual(6, intField[3]);

            open.Dispose();
            directions.Dispose();
            costField.Dispose();
            intField.Dispose();
            targets.Dispose();
        }
    }
}
