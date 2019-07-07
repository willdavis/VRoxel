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
        [Test]
        public void HasData()
        {
            World world = new World();
            world.size = Vector3Int.one;
            world.Initialize();

            Assert.AreEqual(
                new Vector3(0.5f, 0.5f, 0.5f),
                world.data.center
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

        [UnityTest]
        public IEnumerator CanCreateChunks()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/TextureAtlas.mat");
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.chunk = prefab_chunk;

            world.transform.rotation = Quaternion.Euler(0,45,0);
            world.chunkSize = new Vector3Int(32,32,32);
            world.size = new Vector3Int(128,32,128);
            world.scale = 0.5f;

            world.Initialize();
            world.Generate(world.size, Vector3Int.zero);

            // setup a block to be rendered
            Block block = new Block();
            world.blocks.library.Add(1, block);
            world.blocks.texture.material = material;
            world.blocks.texture.size = 0.25f;

            // setup textures for the block
            block.textures.Add(Cube.Direction.Top, Vector2.zero);
            block.textures.Add(Cube.Direction.Bottom, Vector2.zero);
            block.textures.Add(Cube.Direction.North, Vector2.zero);
            block.textures.Add(Cube.Direction.East, Vector2.zero);
            block.textures.Add(Cube.Direction.South, Vector2.zero);
            block.textures.Add(Cube.Direction.West, Vector2.zero);

            for (int x = 0; x < world.size.x / world.chunkSize.x; x++)
            {
                for (int z = 0; z < world.size.z / world.chunkSize.z; z++)
                {
                    for (int y = 0; y < world.size.y / world.chunkSize.y; y++)
                    {
                        Vector3Int point = new Vector3Int(x,y,z);
                        Chunk chunk = world.CreateChunk(point);
                        yield return null;

                        Assert.AreSame(chunk, world.chunks[point]);
                        Assert.AreSame(world.transform, chunk.transform.parent);
                    }
                }
            }

            //System.Threading.Thread.Sleep(1000);
            Object.DestroyImmediate(world);
        }
    }
}
