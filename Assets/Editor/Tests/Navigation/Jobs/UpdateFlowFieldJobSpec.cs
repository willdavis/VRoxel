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
            Vector3Int size = new Vector3Int(1, 1, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<int> intField = new NativeArray<int>(flatSize, Allocator.Persistent);
            NativeArray<byte> flowField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> flowDirections = new NativeArray<Vector3Int>(27, Allocator.Persistent);

            for (int i = 0; i < 27; i++)
                flowDirections[i] = Direction3Int.Directions[i];

            UpdateFlowFieldJob job = new UpdateFlowFieldJob()
            {
                flowDirections = flowDirections,
                flowField = flowField,
                intField = intField
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            Assert.AreEqual((byte)Direction3Int.Name.Zero, flowField[0]);
            Assert.AreEqual((byte)Direction3Int.Name.Zero, flowField[1]);

            intField.Dispose();
            flowField.Dispose();
            flowDirections.Dispose();
        }
    }
}
