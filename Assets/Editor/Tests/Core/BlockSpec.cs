using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BlockSpec
    {
        [Test]
        public void HasName()
        {
            Block block = new Block();
            block.name = "test";
            Assert.AreEqual("test", block.name);
        }

        [Test]
        public void HasTextures()
        {
            Block block = new Block();
            block.textures = new Dictionary<Cube.Direction, Vector2>();
            block.textures.Add(Cube.Direction.Top, Vector2.one);

            Assert.AreEqual(Vector2.one, block.textures[Cube.Direction.Top]);
        }
    }
}
