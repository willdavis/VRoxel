using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Tests
{
    public class WorldSpec
    {
        public class HasChunks
        {
            [UnityTest]
            public IEnumerator CanGetChunkPosition()
            {
                World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");
                World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
                world.Initialize();

                Vector3 position = world.GetChunkPosition(Vector3Int.zero);
                Assert.AreEqual(Vector3.zero, position);

                yield return null;
            }

            [UnityTest]
            public IEnumerator CanCreateChunk()
            {
                Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
                World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

                World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
                world.chunk = prefab_chunk;
                world.Initialize();

                Vector3Int zero = Vector3Int.zero;
                Chunk chunk = world.CreateChunk(zero);

                Assert.AreSame(chunk, world.chunks[zero]);               // check that the chunk was added to the worlds chunks
                Assert.AreSame(world.transform, chunk.transform.parent); // check that the chunk is attached to the worlds transform
                Assert.AreEqual(Vector3.zero, chunk.transform.position); // confirm the chunks position in the Scene

                yield return null;
                Object.DestroyImmediate(chunk);
                Object.DestroyImmediate(world);
            }
        }

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
