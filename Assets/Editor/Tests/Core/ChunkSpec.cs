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

            world.blocks.textures.material = material;
            chunk.Initialize(world, Vector3Int.zero);

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CanGenerateMesh()
        {
            Material material = new Material(Shader.Find("Specular"));

            // setup a world
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.Initialize(); // needed so that the VoxelGrid is initialized

            // setup a chunk
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;
            chunk.runInEditMode = true; // needed so that the Awake() method is called

            // setup a block to be rendered
            Block block = new Block();
            world.data.Set(Vector3Int.zero, 1);
            world.blocks.blocks.Add(1, block);

            // setup textures for the block
            block.textures.Add(Cube.Direction.Top, Vector2.zero);
            block.textures.Add(Cube.Direction.Bottom, Vector2.zero);
            block.textures.Add(Cube.Direction.North, Vector2.zero);
            block.textures.Add(Cube.Direction.East, Vector2.zero);
            block.textures.Add(Cube.Direction.South, Vector2.zero);
            block.textures.Add(Cube.Direction.West, Vector2.zero);

            // setup chunk and generate the mesh
            world.blocks.textures.material = material;
            chunk.Initialize(world, Vector3Int.zero);
            chunk.GenerateMesh();

            // check the generated mesh
            Mesh mesh = chunk.GetComponent<MeshFilter>().sharedMesh;
            Mesh coll = chunk.GetComponent<MeshCollider>().sharedMesh;

            Assert.AreSame(mesh, coll);                       // check both collision and filter meshes
            Assert.AreEqual(24, mesh.vertices.GetLength(0));  // expect 4x6 vertices
            Assert.AreEqual(36, mesh.triangles.GetLength(0)); // expect 6x6 triangles
            Assert.AreEqual(24, mesh.uv.GetLength(0));        // expect 4x6 uv coordinates

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
            yield return null;
        }
    }
}
