using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;
using VRoxel.Terrain.HeightMaps;

namespace TerrainSpecs
{
    public class UpdateHeightMapSpec
    {
        [Test]
        public void CanUpdateTheHeightMap()
        {
            UpdateHeightMap job = new UpdateHeightMap();
            job.voxels = new NativeArray<byte>(8, Allocator.Persistent);
            job.data = new NativeArray<ushort>(4, Allocator.Persistent);
            job.size = new int3(2,2,2);

            job.voxels[0] = 1; // int3(0,0,0)
            job.voxels[7] = 1; // int3(1,1,1)

            job.Execute(0); job.Execute(1);
            job.Execute(2); job.Execute(3);

            Assert.AreEqual(0, job.data[0]); // int2(0,0)
            Assert.AreEqual(1, job.data[3]); // int2(1,1)
            Assert.AreEqual(ushort.MaxValue, job.data[1]); // int2(0,1)
            Assert.AreEqual(ushort.MaxValue, job.data[2]); // int2(1,0)
        }

        [Test]
        public void CanUnflattenAnIndex()
        {
            UpdateHeightMap job = new UpdateHeightMap();
            job.size = new int3(2,2,2);

            Assert.AreEqual(new int2(0,0), job.UnFlatten(0));
            Assert.AreEqual(new int2(0,1), job.UnFlatten(1));
            Assert.AreEqual(new int2(1,0), job.UnFlatten(2));
            Assert.AreEqual(new int2(1,1), job.UnFlatten(3));
        }

        [Test]
        public void CanFlatten3DPoint()
        {
            UpdateHeightMap job = new UpdateHeightMap();
            job.size = new int3(2,2,2);

            Assert.AreEqual(0, job.Flatten(new int3(0,0,0)));
            Assert.AreEqual(1, job.Flatten(new int3(0,0,1)));
            Assert.AreEqual(2, job.Flatten(new int3(0,1,0)));
            Assert.AreEqual(3, job.Flatten(new int3(0,1,1)));
            Assert.AreEqual(4, job.Flatten(new int3(1,0,0)));
            Assert.AreEqual(5, job.Flatten(new int3(1,0,1)));
            Assert.AreEqual(6, job.Flatten(new int3(1,1,0)));
            Assert.AreEqual(7, job.Flatten(new int3(1,1,1)));
        }

        [Test]
        public void CanCheckForSolidBlocks()
        {
            UpdateHeightMap job = new UpdateHeightMap();

            job.size = new int3(1,1,1);
            job.voxels = new NativeArray<byte>(8, Allocator.Persistent);
            job.voxels[0] = 1;

            Assert.AreEqual(true,  job.Solid(new int3(0,0,0)));
            Assert.AreEqual(false, job.Solid(new int3(0,0,1)));
        }
    }
}
