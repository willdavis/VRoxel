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

            manager.blocks.Add(index, block);
            Assert.AreSame(block, manager.blocks[index]);
        }

        [Test]
        public void HasTextures()
        {
            Material material = new Material(Shader.Find("Specular"));
            BlockManager manager = new BlockManager();

            manager.textures.material = material;
            manager.textures.size = 10f;

            Assert.AreSame(material, manager.textures.material);
            Assert.AreEqual(10f, manager.textures.size);
        }
    }
}
