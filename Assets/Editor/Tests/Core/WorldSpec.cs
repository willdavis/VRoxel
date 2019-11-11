using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using VRoxel.Core;

namespace Tests
{
    public class WorldSpec
    {
        public string chunk_prefab_path = "Assets/VRoxel/Core/Prefabs/Chunk.prefab";
        public string world_prefab_path = "Assets/VRoxel/Core/Prefabs/World.prefab";

        [UnityTest]
        public IEnumerator CanCreateChunks()
        {
            // Load assets
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/TextureAtlas.mat");

            // Create blocks and add textures
            Block air = new Block(); air.index = 0;
            Block stone = new Block(); stone.index = 1;
            stone.textures.Add(Cube.Direction.Top, Vector2.one);
            stone.textures.Add(Cube.Direction.Bottom, Vector2.up);
            stone.textures.Add(Cube.Direction.North, Vector2.up);
            stone.textures.Add(Cube.Direction.East, Vector2.up);
            stone.textures.Add(Cube.Direction.South, Vector2.up);
            stone.textures.Add(Cube.Direction.West, Vector2.up);

            // Create a new World
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;

            // Setup the BlockManager
            world.blocks.texture.material = material;
            world.blocks.texture.size = 0.25f;

            // Add Blocks to the library
            world.blocks.library.Add(air.index, air);
            world.blocks.library.Add(stone.index, stone);

            // Configure the World
            world.chunk = prefab_chunk;
            world.transform.rotation = Quaternion.Euler(0,45,0);
            world.chunkSize = new Vector3Int(32,32,32);
            world.size = new Vector3Int(256,32,256);
            world.scale = 0.25f;
            world.seed = 1337;

            // Initialize and generate world data
            world.Initialize();
            Generate(world, world.size, Vector3Int.zero);

            // Load all chunks in the world
            world.chunks.Load(world.chunks.max, Vector3Int.zero);
            yield return null;

            // Edit the world
            Vector3Int start = new Vector3Int(0,10,0);
            Vector3Int end = new Vector3Int(128,32,128);
            Vector3Int center = new Vector3Int(128,16,128);

            WorldEditor.Set(world, start, end, air.index);
            WorldEditor.Set(world, center, 16, stone.index);

            Object.DestroyImmediate(world);
        }


        /// <summary>
        /// Generate world data within the given bounds.
        /// Any points outside the world will be skipped.
        /// </summary>
        /// <param name="size">The number of voxels to generate</param>
        /// <param name="offset">The offset from the world origin</param>
        public void Generate(World world, Vector3Int size, Vector3Int offset)
        {
            Vector3Int point = Vector3Int.zero;
            for (int x = 0; x < size.x; x++)
            {
                point.x = x + offset.x;
                for (int z = 0; z < size.z; z++)
                {
                    point.z = z + offset.z;
                    for (int y = 0; y < size.y; y++)
                    {
                        point.y = y + offset.y;
                        world.data.Set(point, 1);
                    }
                }
            }
        }
    }
}
