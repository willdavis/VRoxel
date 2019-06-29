using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BlockManagerSpec
    {
        [Test]
        public void HasBlocks()
        {
            byte index = 0;
            Block block = new Block();
            BlockManager manager = new BlockManager();

            manager.library.Add(index, block);
            Assert.AreSame(block, manager.library[index]);
        }

        [Test]
        public void HasTextures()
        {
            Material material = new Material(Shader.Find("Specular"));
            BlockManager manager = new BlockManager();

            manager.texture.material = material;
            manager.texture.size = 10f;

            Assert.AreSame(material, manager.texture.material);
            Assert.AreEqual(10f, manager.texture.size);
        }
    }
}
