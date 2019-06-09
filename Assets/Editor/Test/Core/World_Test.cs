using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class World_Test
    {
        // A Test behaves as an ordinary method
        [Test]
        public void World_TestSimplePasses()
        {
            World test = new World();
            test.size = new Vector3Int(1,2,3);

            Assert.AreEqual(test.size.x, 1);
            Assert.AreEqual(test.size.y, 2);
            Assert.AreEqual(test.size.z, 3);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator World_TestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
