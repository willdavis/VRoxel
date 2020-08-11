using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Terrain.HeightMaps
{
    /// <summary>
    /// Updates the height value of each position in a height map
    /// </summary>
    [BurstCompile]
    public struct UpdateHeightMap : IJobParallelFor
    {
        public void Execute(int i)
        {
            
        }
    }
}