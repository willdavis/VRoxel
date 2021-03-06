﻿using NUnit.Framework;
using UnityEngine;

using VRoxel.Core;

namespace CoreSpecs
{
    public class VoxelGridSpec
    {
        [Test]
        public void HasCenter()
        {
            Vector3Int size = new Vector3Int(10,8,6);
            VoxelGrid data = new VoxelGrid(size);

            Assert.AreEqual(5f, data.center.x);
            Assert.AreEqual(4f, data.center.y);
            Assert.AreEqual(3f, data.center.z);

            data.Dispose();
        }

        [Test]
        public void CanGetPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);
            data.Set(Vector3Int.zero, 1);

            Assert.AreEqual(1, data.Get(Vector3Int.zero)); // check point inside the world
            Assert.AreEqual(0, data.Get(Vector3Int.one));  // check point outside the world

            data.Dispose();
        }

        [Test]
        public void CanSetPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);

            data.Set(Vector3Int.zero, 12); // set point inside the world
            Assert.AreEqual(12, data.Get(Vector3Int.zero));

            data.Set(Vector3Int.one, 12);  // set point outside the world
            Assert.AreEqual(0, data.Get(Vector3Int.one));

            data.Dispose();
        }

        [Test]
        public void CanContainPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);

            Assert.AreEqual(true, data.Contains(Vector3Int.zero));   // check point inside the world
            Assert.AreEqual(false, data.Contains(Vector3Int.right)); // check point outside the world

            data.Dispose();
        }
    }
}
