using NUnit.Framework;
using UnityEngine;

namespace Tests
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
        }

        [Test]
        public void CanGetPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);
            data.Set(Vector3Int.zero, 1);

            Assert.AreEqual(1, data.Get(Vector3Int.zero)); // check point inside the world
            Assert.AreEqual(0, data.Get(Vector3Int.one));  // check point outside the world
        }

        [Test]
        public void CanSetPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);

            data.Set(Vector3Int.zero, 12); // set point inside the world
            Assert.AreEqual(12, data.Get(Vector3Int.zero));

            data.Set(Vector3Int.one, 12);  // set point outside the world
            Assert.AreEqual(0, data.Get(Vector3Int.one));
        }

        [Test]
        public void CanContainPoint()
        {
            VoxelGrid data = new VoxelGrid(Vector3Int.one);

            Assert.AreEqual(true, data.Contains(Vector3Int.zero));   // check point inside the world
            Assert.AreEqual(false, data.Contains(Vector3Int.right)); // check point outside the world
        }

        [Test]
        public void CanSetRange()
        {
            Vector3Int size = new Vector3Int(2,2,2);
            VoxelGrid data = new VoxelGrid(size);
            data.Set(Vector3Int.zero, Vector3Int.one, 1);

            Assert.AreEqual(1, data.Get(Vector3Int.zero));
            Assert.AreEqual(1, data.Get(Vector3Int.one));
            Assert.AreEqual(1, data.Get(Vector3Int.up));
        }

        [Test]
        public void CanSetNeighborhood()
        {
            Vector3Int size = new Vector3Int(3,3,3);
            VoxelGrid data = new VoxelGrid(size);
            data.Set(Vector3Int.one, 1, 1);

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Assert.AreEqual(1, data.Get(x,y,z));
                    }
                }
            }
        }
    }
}
