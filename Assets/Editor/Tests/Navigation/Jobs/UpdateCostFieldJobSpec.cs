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

            for (int i = 0; i < 27; i++)
                directions[i] = Direction3Int.Directions[i];

            voxels[0] = 1;  // solid
            voxels[1] = 0;  // air
            voxels[2] = 0;  // air
            voxels[3] = 0;  // air

            UpdateCostFieldJob job = new UpdateCostFieldJob()
            {
                directions = directions,
                costField = costField,
                voxels = voxels,
                size = size
            };

            JobHandle handle = job.Schedule(flatSize, 1);
            handle.Complete();

            voxels.Dispose();
            costField.Dispose();
            directions.Dispose();
        }
    }
}