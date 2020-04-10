using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Navigation;
using NUnit.Framework;

namespace NavigationJobSpecs
{
    public class ClearIntFieldJobSpec
    {
        [Test]
        public void ClearsTheField()
        {
            int3 size = new int3(2, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<ushort> intField = new NativeArray<ushort>(flatSize, Allocator.Persistent);

            ClearIntFieldJob job = new ClearIntFieldJob()
            {
                intField = intField
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            for (int i = 0; i < flatSize; i++)
            {
                Assert.AreEqual(ushort.MaxValue, intField[i]);
            }

            intField.Dispose();
        }
    }
}