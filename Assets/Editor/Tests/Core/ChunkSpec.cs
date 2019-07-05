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
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.chunkSize = new Vector3Int(2,2,2);
            world.size = new Vector3Int(2,2,2);
            world.Initialize(); // needed so that the VoxelGrid is initialized

            // setup a chunk
            Chunk chunk = UnityEngine.Object.Instantiate(prefab_chunk, Vector3.zero, Quaternion.identity) as Chunk;
            chunk.transform.position += new Vector3(-0.5f, -0.5f, -0.5f); // align the chunk with the world size (2,2,2)

            // generate data for the world
            world.data.Set(Vector3Int.up, 1);
            world.data.Set(Vector3Int.zero, 1);
            world.data.Set(Vector3Int.right, 1);
            world.data.Set(Vector3Int.one, 1);

            // setup a block to be rendered
            Block block = new Block();
            world.blocks.library.Add(1, block);
            world.blocks.texture.material = material;
            world.blocks.texture.size = 1f;

            // setup textures for the block
            block.textures.Add(Cube.Direction.Top, Vector2.zero);
            block.textures.Add(Cube.Direction.Bottom, Vector2.zero);
            block.textures.Add(Cube.Direction.North, Vector2.zero);
            block.textures.Add(Cube.Direction.East, Vector2.zero);
            block.textures.Add(Cube.Direction.South, Vector2.zero);
            block.textures.Add(Cube.Direction.West, Vector2.zero);

            // setup chunk and generate the mesh
            chunk.Initialize(world, Vector3Int.zero);
            chunk.GenerateMesh();

            yield return null; // skip a frame to render the chunk
            //System.Threading.Thread.Sleep(2000); // sleep so we can see it

            // check the generated mesh
            Mesh mesh = chunk.GetComponent<MeshFilter>().sharedMesh;
            Mesh coll = chunk.GetComponent<MeshCollider>().sharedMesh;

            Assert.AreSame(mesh, coll);
            Assert.AreEqual(80, mesh.uv.GetLength(0));
            Assert.AreEqual(80, mesh.vertices.GetLength(0));
            Assert.AreEqual(120, mesh.triangles.GetLength(0));

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }
    }
}
