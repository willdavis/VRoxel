using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
using NUnit.Framework;

namespace CoreJobSpecs
{
    public class EditVoxelJobSpec
    {
        [Test]
        public void UpdatesVoxelsInARectangle()
        {
            int3 size = new int3(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

            EditVoxelJob job = new EditVoxelJob()
            {
                block = 2,
                size = size,
                voxels = voxels,
                end = new int3(0,1,0),
                start = new int3(0,0,0),
            };
            JobHandle handle = job.Schedule();
            handle.Complete();

            Assert.AreEqual(2, voxels[0]);
            Assert.AreEqual(0, voxels[1]);
            Assert.AreEqual(2, voxels[2]);
            Assert.AreEqual(0, voxels[3]);

            voxels.Dispose();
        }

        [Test]
        public void SkipsOutOfBoundPositions()
        {
            int3 size = new int3(1, 1, 1);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

            EditVoxelJob job = new EditVoxelJob()
            {
                block = 2,
                size = size,
                voxels = voxels,
                end = new int3(0,1,0),
                start = new int3(0,0,0),
            };
            JobHandle handle = job.Schedule();
            handle.Complete();

            Assert.AreEqual(2, voxels[0]);
            Assert.DoesNotThrow(job.Execute);

            voxels.Dispose();
        }
    }
}
