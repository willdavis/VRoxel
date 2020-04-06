using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace VRoxel.Navigation
{
    /// <summary>
    /// Updates the integration field
    /// </summary>
    public struct UpdateIntFieldJob : IJob
    {
        [ReadOnly]
        public NativeArray<byte> costField;

        [WriteOnly]
        public NativeArray<ushort> intField;

        public void Execute()
        {
            
        }
    }
}