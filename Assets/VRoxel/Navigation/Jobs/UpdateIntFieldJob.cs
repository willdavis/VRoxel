﻿using UnityEngine;
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
        public Vector3Int size;
        public Vector3Int goal;

        [ReadOnly]
        public NativeArray<byte> costField;

        [WriteOnly]
        public NativeArray<ushort> intField;

        public void Execute()
        {
            
        }

        /// <summary>
        /// Calculate an array index from a Vector3Int point
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(Vector3Int point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }

        /// <summary>
        /// Calculate a Vector3Int point from an array index
        /// </summary>
        public Vector3Int UnFlatten(int index)
        {
            int x = index / (size.y * size.z);
            int y = (index - x * size.y * size.z) / size.z;
            int z = index - x * size.y * size.z - y * size.z;
            return new Vector3Int(x,y,z);
        }

        /// <summary>
        /// Test if a point is inside the flow field
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public bool OutOfBounds(Vector3Int point)
        {
            if (point.x < 0 || point.x >= size.x) { return true; }
            if (point.y < 0 || point.y >= size.y) { return true; }
            if (point.z < 0 || point.z >= size.z) { return true; }
            return false;
        }
    }
}