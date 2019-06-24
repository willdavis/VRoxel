﻿using System.Collections;
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
                world.data.Center()
            );
        }

        [Test]
        public void CanGenerateDefaultData()
        {
            Vector3Int size = new Vector3Int(10, 10, 10);
            Vector3Int zero = Vector3Int.zero;

            World world = new World();
            world.size = size;
            world.Initialize();
            world.Generate(size, zero);

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Assert.AreEqual(1, world.data.Get(x,y,z));
                    }
                }
            }
        }
    }
}
