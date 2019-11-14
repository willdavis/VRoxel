using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using VRoxel.Core;

namespace CoreSpecs
{
    public class ChunkSpec
    {
        public string chunk_prefab_path = "Assets/VRoxel/Core/Prefabs/Chunk.prefab";
        public string world_prefab_path = "Assets/VRoxel/Core/Prefabs/World.prefab";

        [UnityTest]
        public IEnumerator CanInitialize()
        {
            Material material = new Material(Shader.Find("Specular"));

            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;

            world.blocks.texture.material = material;
            chunk.Initialize(world, Vector3Int.zero);

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CanDisableCollision()
        {
            // create a new chunk
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;
            World world = CreateWorld();

            chunk.collidable = false;
            chunk.runInEditMode = true; // needed for integration tests
            chunk.Initialize(world, Vector3Int.zero);
            chunk.GenerateMesh();

            Mesh collider = chunk.GetComponent<MeshCollider>().sharedMesh;
            Assert.AreSame(null, collider);

            yield return null;

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [UnityTest]
        public IEnumerator CanGenerateMesh()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World world = CreateWorld();

            // setup chunk and generate the mesh
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;
            chunk.runInEditMode = true; // needed for integration tests

            chunk.Initialize(world, Vector3Int.zero);
            chunk.GenerateMesh();

            yield return null; // skip a frame to render the chunk
            //System.Threading.Thread.Sleep(2000); // sleep so we can see it

            // check the generated mesh
            Mesh mesh = chunk.GetComponent<MeshFilter>().sharedMesh;
            Mesh coll = chunk.GetComponent<MeshCollider>().sharedMesh;

            Assert.AreSame(mesh, coll);
            Assert.AreEqual(600, mesh.uv.GetLength(0));
            Assert.AreEqual(600, mesh.vertices.GetLength(0));
            Assert.AreEqual(900, mesh.triangles.GetLength(0));

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        /// helper functions
        /// -----------------------------
        World CreateWorld()
        {
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/TextureAtlas.mat");

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            Vector3Int size = new Vector3Int(5,5,5);

            world.chunkSize = size;
            world.size = size;
            world.scale = 0.5f;
            world.Initialize();

            for (int x = 0; x < world.size.x; x++)
            {
                for (int y = 0; y < world.size.y; y++)
                {
                    for (int z = 0; z < world.size.z; z++)
                    {
                        world.data.Set(x,y,z,1);
                    }
                }
            }

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

            return world;
        }
    }
}
