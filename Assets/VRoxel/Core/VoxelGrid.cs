﻿using UnityEngine;
using Unity.Collections;

namespace VRoxel.Core
{
    /// <summary>
    /// Contains voxel data, configuration, and helper methods
    /// </summary>
    public class VoxelGrid
    {
        private Vector3 _center;
        private Vector3Int _size;

        private NativeArray<byte> _voxels;
        public NativeArray<byte> voxels { get { return _voxels; } }

        public VoxelGrid(Vector3Int size)
        {
            _size = size;
            _center = new Vector3(size.x / 2f, size.y / 2f, size.z / 2f);
            _voxels = new NativeArray<byte>(size.x * size.y * size.z, Allocator.Persistent);
        }

        public void Dispose()
        {
            _voxels.Dispose();
        }

        /// <summary>
        /// The center point of the voxel grid
        /// </summary>
        public Vector3 center { get { return _center; } }

        /// <summary>
        /// Tests if a point is inside the voxel grid
        /// </summary>
        /// <param name="point">The point to compare</param>
        public bool Contains(Vector3Int point)
        {
            if (point.x < 0 || point.x >= _size.x) { return false; }
            if (point.y < 0 || point.y >= _size.y) { return false; }
            if (point.z < 0 || point.z >= _size.z) { return false; }
            return true;
        }

        /// <summary>
        /// Convert (x,y,z) coordinates to an array index
        /// </summary>
        public int Flatten(int x, int y, int z)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (x * _size.y * _size.z) + (y * _size.z) + z;
        }

        /// <summary>
        /// Unsafely get a block type from the voxel grid cache
        /// </summary>
        /// <param name="x">the X coordinate</param>
        /// <param name="y">the Y coordinate</param>
        /// <param name="z">the Z coordinate</param>
        public byte Get(int x, int y, int z) { return _voxels[Flatten(x,y,z)]; }

        /// <summary>
        /// Safely get a block type from the voxel grid cache
        /// </summary>
        /// <param name="point">A point inside the world</param>
        public byte Get(Vector3Int point)
        {
            if (!Contains(point)) { return 0; }
            return Get(point.x, point.y, point.z);
        }

        /// <summary>
        /// Unsafely set a block index in the voxel grid cache
        /// </summary>
        /// <param name="x">the X coordinate</param>
        /// <param name="y">the Y coordinate</param>
        /// <param name="z">the Z coordinate</param>
        /// <param name="block">The block index to set</param>
        public void Set(int x, int y, int z, byte block)
        {
            _voxels[Flatten(x,y,z)] = block;
        }

        /// <summary>
        /// Safely set a block in the voxel grid cache
        /// </summary>
        /// <param name="point">A point inside the voxel grid</param>
        /// <param name="block">The block index to set</param>
        public void Set(Vector3Int point, byte block)
        {
            if (!Contains(point)) { return; }
            Set(point.x, point.y, point.z, block);
        }
    }
}
