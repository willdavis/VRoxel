using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WorldDataSpec
    {
        [Test]
        public void HasCenter()
        {
            Vector3Int size = new Vector3Int(10,8,6);
            WorldData data = new WorldData(size);

            Assert.AreEqual(5f, data.Center().x);
            Assert.AreEqual(4f, data.Center().y);
            Assert.AreEqual(3f, data.Center().z);
        }

        [Test]
        public void CanGetPoint()
        {
            Vector3Int size = new Vector3Int(1,1,1);
            WorldData data = new WorldData(size);

            Assert.AreEqual(0, data.Get(Vector3Int.zero));
        }

        [Test]
        public void CanSetPoint()
        {
            Vector3Int size = new Vector3Int(1,1,1);
            WorldData data = new WorldData(size);

            data.Set(Vector3Int.zero, 12);
            Assert.AreEqual(12, data.Get(Vector3Int.zero));
        }

        [Test]
        public void CanContainPoint()
        {
            Vector3Int size = new Vector3Int(1,1,1);
            WorldData data = new WorldData(size);

            Assert.AreEqual(true, data.Contains(Vector3Int.zero));
            Assert.AreEqual(false, data.Contains(Vector3Int.right));
        }
    }
}
