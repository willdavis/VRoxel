using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using VRoxel.Core;
using NUnit.Framework;

namespace CoreJobSpecs
{
    public class EditVoxelSphereJobSpec
    {
        [Test]
        public void CanEditInASphere()
        {
            int3 size = new int3(3, 3, 3);
            int3 center = new int3(size.x / 2, size.y / 2, size.z / 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

            EditVoxelSphereJob job = new EditVoxelSphereJob()
            {
                block = 1,
                radius = 1f,
                voxels = voxels,
                worldSize = size,
                position = center
            };
            JobHandle handle = job.Schedule();
            handle.Complete();

            Assert.AreEqual(0, voxels[0]);
            Assert.AreEqual(0, voxels[1]);
            Assert.AreEqual(0, voxels[2]);

            Assert.AreEqual(0, voxels[3]);
            Assert.AreEqual(1, voxels[4]);
            Assert.AreEqual(0, voxels[5]);

            Assert.AreEqual(0, voxels[6]);
            Assert.AreEqual(0, voxels[7]);
            Assert.AreEqual(0, voxels[8]);

            Assert.AreEqual(0, voxels[9]);
            Assert.AreEqual(1, voxels[10]);
            Assert.AreEqual(0, voxels[11]);

            Assert.AreEqual(1, voxels[12]);
            Assert.AreEqual(1, voxels[13]);
            Assert.AreEqual(1, voxels[14]);

            Assert.AreEqual(0, voxels[15]);
            Assert.AreEqual(1, voxels[16]);
            Assert.AreEqual(0, voxels[17]);

            Assert.AreEqual(0, voxels[18]);
            Assert.AreEqual(0, voxels[19]);
            Assert.AreEqual(0, voxels[20]);

            Assert.AreEqual(0, voxels[21]);
            Assert.AreEqual(1, voxels[22]);
            Assert.AreEqual(0, voxels[23]);

            Assert.AreEqual(0, voxels[24]);
            Assert.AreEqual(0, voxels[25]);
            Assert.AreEqual(0, voxels[26]);

            voxels.Dispose();
        }

        [Test]
        public void SkipsOutOfBoundPositions()
        {
            int3 size = new int3(3, 3, 3);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

            EditVoxelSphereJob job = new EditVoxelSphereJob()
            {
                block = 1,
                radius = 5f,
                voxels = voxels,
                worldSize = size,
                position = int3.zero
            };
            JobHandle handle = job.Schedule();
            handle.Complete();

            Assert.DoesNotThrow(job.Execute);

            voxels.Dispose();
        }
    }
}
