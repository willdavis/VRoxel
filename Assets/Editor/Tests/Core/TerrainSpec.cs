using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TerrainSpec
    {
        [Test]
        public void CanGetHeight()
        {
            int seed = 0;
            Terrain terrain = new Terrain(seed);
            Assert.AreEqual(0, terrain.GetHeight(0,0,1));
        }
    }
}
