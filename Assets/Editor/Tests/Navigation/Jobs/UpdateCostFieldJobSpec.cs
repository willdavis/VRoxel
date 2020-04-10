using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Mathematics;

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
            int3 size = new int3(1, 2, 2);
            int flatSize = size.x * size.y * size.z;

            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<byte> costField = new NativeArray<byte>(flatSize, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(27, Allocator.Persistent);
            NativeArray<int> directionMask = new NativeArray<int>(4, Allocator.Persistent);
            NativeArray<VRoxel.Navigation.Block> blocks = new NativeArray<VRoxel.Navigation.Block>(2, Allocator.Persistent);

            VRoxel.Navigation.Block airBlock = new VRoxel.Navigation.Block();
            airBlock.solid = false;
            airBlock.cost = 2;
            blocks[0] = airBlock;

            VRoxel.Navigation.Block solidBlock = new VRoxel.Navigation.Block();
            solidBlock.solid = true;
            solidBlock.cost = 1;
            blocks[1] = solidBlock;

            voxels[0] = 1;  // solid block
            voxels[1] = 0;  // air block
            voxels[2] = 0;  // air block
            voxels[3] = 0;  // air block

            directionMask[0] = 3;
            directionMask[1] = 5;
            directionMask[2] = 7;
            directionMask[3] = 9;

            for (int i = 0; i < 27; i++)
            {
                Vector3Int dir = Direction3Int.Directions[i];
                directions[i] = new int3(dir.x, dir.y, dir.z);
            }

            UpdateCostFieldJob job = new UpdateCostFieldJob()
            {
                directionMask = directionMask,
                directions = directions,
                costField = costField,
                voxels = voxels,
                blocks = blocks,
                size = size,
                height = 1
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            Assert.AreEqual(1, costField[0]);   // walkable node
            Assert.AreEqual(2, costField[1]);   // climbable node
            Assert.AreEqual(255, costField[2]); // obstructed node
            Assert.AreEqual(255, costField[3]); // obstructed node

            blocks.Dispose();
            voxels.Dispose();
            costField.Dispose();
            directions.Dispose();
            directionMask.Dispose();
        }
    }
}