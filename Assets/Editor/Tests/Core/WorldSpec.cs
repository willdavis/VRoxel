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
        [UnityTest]
        public IEnumerator CanCreateChunks()
        {
            // Load assets
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/TextureAtlas.mat");
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

            // Create a Block and add textures
            Block block = new Block();
            block.textures.Add(Cube.Direction.Top, Vector2.one);
            block.textures.Add(Cube.Direction.Bottom, Vector2.up);
            block.textures.Add(Cube.Direction.North, Vector2.up);
            block.textures.Add(Cube.Direction.East, Vector2.up);
            block.textures.Add(Cube.Direction.South, Vector2.up);
            block.textures.Add(Cube.Direction.West, Vector2.up);

            // Create a new World
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;

            // Setup the BlockManager
            world.blocks.texture.material = material;
            world.blocks.texture.size = 0.25f;

            // Add Blocks to the library
            world.blocks.library.Add(1, block);
            //world.blocks.library.Add(2, block);
            //world.blocks.library.Add(3, block);
            //...

            // Configure the World
            world.chunk = prefab_chunk;
            world.transform.rotation = Quaternion.Euler(0,45,0);
            world.chunkSize = new Vector3Int(32,32,32);
            world.size = new Vector3Int(256,32,256);
            world.scale = 0.25f;
            world.seed = 1337;

            // Initialize and generate world data
            world.Initialize();
            world.Generate(world.size, Vector3Int.zero);

            // Load all chunks in the world
            world.chunks.Load(world.chunks.max, Vector3Int.zero);
            yield return null;

            //System.Threading.Thread.Sleep(2000);
            Object.DestroyImmediate(world);
        }
    }
}
