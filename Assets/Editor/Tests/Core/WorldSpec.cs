using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WorldSpec
    {
        [Test]
        public void HasData()
        {
            World world = new World();
            world.size = Vector3Int.one;
            world.Initialize();

            Assert.AreEqual(
                new Vector3(0.5f, 0.5f, 0.5f),
                world.Data().Center()
            );
        }
    }
}
