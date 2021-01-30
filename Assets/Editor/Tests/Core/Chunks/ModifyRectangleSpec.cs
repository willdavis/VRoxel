using NUnit.Framework;
using VRoxel.Core.Chunks;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace CoreChunksSpecs
{
    public class ModifyRectangleSpec
    {
        [Test]
        public void UpdatesVoxelsInARectangle()
        {
            int3 offset = int3.zero;
            int3 size = new int3(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Block> blockLibrary = new NativeArray<Block>(2, Allocator.Persistent);
            NativeArray<byte> blocksToIgnore = new NativeArray<byte>(0, Allocator.Persistent);

            blockLibrary[0] = new Block() { editable = true };
            blockLibrary[1] = new Block() { editable = true };

            ModifyRectangle job = new ModifyRectangle()
            {
                block = 1,
                chunkOffset = offset,
                chunkSize = size,
                voxels = voxels,
                end = new int3(0,1,0),
                start = new int3(0,0,0),
                blockLibrary = blockLibrary,
                blocksToIgnore = blocksToIgnore,
            };
            job.Schedule().Complete();

            Assert.AreEqual(1, voxels[0]);
            Assert.AreEqual(0, voxels[1]);
            Assert.AreEqual(1, voxels[2]);
            Assert.AreEqual(0, voxels[3]);

            voxels.Dispose();
            blockLibrary.Dispose();
            blocksToIgnore.Dispose();
        }
    }
}
