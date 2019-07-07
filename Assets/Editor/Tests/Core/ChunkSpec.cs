using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Tests
{
    public class ChunkSpec
    {
        [UnityTest]
        public IEnumerator CanInitialize()
        {
            Material material = new Material(Shader.Find("Specular"));

            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;

            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;

            world.blocks.texture.material = material;
            chunk.Initialize(world, Vector3Int.zero);

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CanGenerateMesh()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/TextureAtlas.mat");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");

            // setup a world
            Vector3Int size = new Vector3Int(5,5,5);
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.chunkSize = size;
            world.size = size;
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

            // setup chunk and generate the mesh
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;
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
    }
}
