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
            world.size = new Vector3Int(256,32,256);
            world.scale = 0.25f;
            world.seed = 1337;

            world.Initialize();
            world.Generate(world.size, Vector3Int.zero);

            // setup a block to be rendered
            Block block = new Block();
            world.blocks.library.Add(1, block);
            world.blocks.texture.material = material;
            world.blocks.texture.size = 0.25f;

            // setup textures for the block
            block.textures.Add(Cube.Direction.Top, Vector2.one);
            block.textures.Add(Cube.Direction.Bottom, Vector2.up);
            block.textures.Add(Cube.Direction.North, Vector2.up);
            block.textures.Add(Cube.Direction.East, Vector2.up);
            block.textures.Add(Cube.Direction.South, Vector2.up);
            block.textures.Add(Cube.Direction.West, Vector2.up);

            for (int x = 0; x < world.size.x / world.chunkSize.x; x++)
            {
                for (int z = 0; z < world.size.z / world.chunkSize.z; z++)
                {
                    for (int y = 0; y < world.size.y / world.chunkSize.y; y++)
                    {
                        Vector3Int index = new Vector3Int(x,y,z);
                        Chunk chunk = world.chunks.Create(index);
                        yield return null;

                        Assert.AreSame(chunk, world.chunks.Get(index));
                        Assert.AreSame(world.transform, chunk.transform.parent);
                    }
                }
            }

            //System.Threading.Thread.Sleep(2000);
            Object.DestroyImmediate(world);
        }
    }
}
