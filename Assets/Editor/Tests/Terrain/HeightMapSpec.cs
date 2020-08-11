using NUnit.Framework;
using VRoxel.Terrain;
using UnityEngine;

namespace TerrainSpecs
{
    public class HeightMapSpec
    {
        [Test]
        public void CanReadHeightValues()
        {
            GameObject obj = new GameObject();
            HeightMap map = obj.AddComponent<HeightMap>();

            map.size = Vector3Int.one;
            map.Initialize();

            ushort max = ushort.MaxValue;
            Assert.AreEqual(0, map.Read(0,0));
            Assert.AreEqual(max, map.Read(1,1));

            map.Dispose();
        }

        [Test]
        public void CanCheckIfContainsPoint()
        {
            GameObject obj = new GameObject();
            HeightMap map = obj.AddComponent<HeightMap>();

            map.size = Vector3Int.one * 2;
            Assert.AreEqual(true, map.Contains(0,0));
            Assert.AreEqual(true, map.Contains(1,1));

            Assert.AreEqual(false, map.Contains(2,2));
            Assert.AreEqual(false, map.Contains(-1,-1));
        }

        [Test]
        public void CanFlattenToIndex()
        {
            GameObject obj = new GameObject();
            HeightMap map = obj.AddComponent<HeightMap>();

            map.size = Vector3Int.one * 3;
            Assert.AreEqual(0, map.Flatten(0,0));
            Assert.AreEqual(1, map.Flatten(0,1));
            Assert.AreEqual(2, map.Flatten(0,2));
            Assert.AreEqual(3, map.Flatten(1,0));
            Assert.AreEqual(4, map.Flatten(1,1));
            Assert.AreEqual(5, map.Flatten(1,2));
            Assert.AreEqual(6, map.Flatten(2,0));
            Assert.AreEqual(7, map.Flatten(2,1));
            Assert.AreEqual(8, map.Flatten(2,2));
        }

        [Test]
        public void CanRefreshTheHeightMap()
        {
            GameObject obj = new GameObject();
            HeightMap map = obj.AddComponent<HeightMap>();

            map.size = Vector3Int.one;
            Unity.Jobs.JobHandle handle = map.Refresh();

            Assert.IsInstanceOf(typeof(Unity.Jobs.JobHandle), handle);
            Assert.AreEqual(true, handle.IsCompleted);
        }
    }
}
