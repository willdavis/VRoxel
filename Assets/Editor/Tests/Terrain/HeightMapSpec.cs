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
        public void CanRefreshTheHeightMap()
        {
            GameObject obj = new GameObject();
            HeightMap map = obj.AddComponent<HeightMap>();

            map.size = Vector3Int.one;
            Unity.Jobs.JobHandle handle = map.Refresh();

            Assert.IsInstanceOf(typeof(Unity.Jobs.JobHandle), handle);
            Assert.AreEqual(false, handle.IsCompleted);

            handle.Complete();
            Assert.AreEqual(true, handle.IsCompleted);
        }
    }
}
