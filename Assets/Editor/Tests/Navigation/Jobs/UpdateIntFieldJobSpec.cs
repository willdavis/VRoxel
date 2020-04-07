using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Navigation;
using VRoxel.Core;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace NavigationSpecs
{
    public class UpdateIntFieldJobSpec
    {
        [Test]
        public void UpdatesTheIntegrationField()
        {
            Vector3Int size = new Vector3Int(1, 1, 4);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> costField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> directions = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            for (int i = 0; i < flatSize; i++)
            {
                costField[i] = 2;
                intField[i] = ushort.MaxValue;
            }

            for (int i = 0; i < 27; i++)
                directions[i] = Direction3Int.Directions[i];
            
            UpdateIntFieldJob job = new UpdateIntFieldJob()
            {
                size = size,
                goal = Vector3Int.zero,
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

            costField.Dispose();
            intField.Dispose();
            directions.Dispose();
        }
    }
}
