using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

using VRoxel.Core;
using VRoxel.Navigation;
using NUnit.Framework;

namespace NavigationJobSpecs
{
    public class UpdateCostFieldJobSpec
    {
        [Test]
        public void UpdatesTheField()
        {
            Vector3Int size = new Vector3Int(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<byte> costField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<Vector3Int> directions = new NativeArray<Vector3Int>(27, Allocator.Persistent);
            NativeArray<VRoxel.Navigation.Block> blocks = new NativeArray<VRoxel.Navigation.Block>(2, Allocator.Persistent);

            VRoxel.Navigation.Block airBlock = new VRoxel.Navigation.Block();
            airBlock.solid = false;
            blocks[0] = airBlock;

            VRoxel.Navigation.Block solidBlock = new VRoxel.Navigation.Block();
            solidBlock.solid = true;
            blocks[1] = solidBlock;

            voxels[0] = 1;  // solid block
            voxels[1] = 0;  // air block
            voxels[2] = 0;  // air block
            voxels[3] = 0;  // air block

            for (int i = 0; i < 27; i++)
                directions[i] = Direction3Int.Directions[i];

            UpdateCostFieldJob job = new UpdateCostFieldJob()
            {
                directions = directions,
                costField = costField,
                voxels = voxels,
                blocks = blocks,
                size = size
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            Assert.AreEqual(1, costField[0]);   // walkable node
            Assert.AreEqual(255, costField[1]); // obstructed node
            Assert.AreEqual(255, costField[2]); // obstructed node
            Assert.AreEqual(255, costField[3]); // obstructed node

            blocks.Dispose();
            voxels.Dispose();
            costField.Dispose();
            directions.Dispose();
        }
    }
}