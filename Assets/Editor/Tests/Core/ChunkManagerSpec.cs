using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using VRoxel.Core;

namespace CoreSpecs
{
    public class ChunkManagerSpec
    {
        public string chunk_prefab_path = "Assets/VRoxel/Core/Prefabs/Chunk.prefab";
        public string world_prefab_path = "Assets/VRoxel/Core/Prefabs/World.prefab";

        [Test]
        public void CanContainIndexes()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);
            ChunkManager manager = new ChunkManager(prefab_world, prefab_chunk);

            Assert.AreEqual(true, manager.Contains(Vector3Int.zero));
            Assert.AreEqual(false, manager.Contains(Vector3Int.one));
        }

        [Test]
        public void CanCreateChunks()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);
            Assert.AreEqual(null, manager.Get(Vector3Int.one));  // out of bounds
            Assert.AreEqual(null, manager.Get(Vector3Int.zero)); // no chunk is present

            Chunk chunk = manager.Create(Vector3Int.zero);
            Assert.AreSame(chunk, manager.Get(Vector3Int.zero)); // confirm chunk was cached

            world.data.Dispose();
            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanCreateChunksWithoutCollision()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);
            manager.collidable = false;

            Chunk chunk = manager.Create(Vector3Int.zero);
            Mesh collider = chunk.GetComponent<MeshCollider>().sharedMesh;
            Assert.AreEqual(false, chunk.collidable);
            Assert.AreEqual(null, collider);

            world.data.Dispose();
            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanUpdateChunk()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.size = new Vector3Int(2,2,2);
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);
            Chunk chunk = manager.Create(Vector3Int.zero);

            manager.Update(Vector3Int.zero);
            Assert.AreEqual(true, chunk.stale);

            Assert.DoesNotThrow(
                () => manager.Update(Vector3Int.one),
                "The given key was not present in the dictionary."
            );

            Assert.DoesNotThrow(
                () => manager.Update(new Vector3Int(5,5,5)),
                "The given key was not present in the dictionary."
            );

            world.data.Dispose();
            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanUpdateChunkNeighbors()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.size = new Vector3Int(2,2,2);
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);
            Chunk chunk = manager.Create(Vector3Int.zero);
            Chunk control = manager.Create(Vector3Int.one);
            Chunk neighbor = manager.Create(Vector3Int.up);

            manager.UpdateFrom(Vector3Int.zero);

            Assert.AreEqual(true, chunk.stale);
            Assert.AreEqual(true, neighbor.stale);
            Assert.AreEqual(false, control.stale);

            world.data.Dispose();
            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(control);
            Object.DestroyImmediate(neighbor);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanBatchCreateChunks()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            Vector3Int size = new Vector3Int(2,2,2);
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.size = size;
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);

            manager.Load(size, Vector3Int.zero);
            foreach (Chunk chunk in manager.all) { Assert.AreEqual(false, chunk.stale); }

            // clean up
            world.data.Dispose();
            foreach (Chunk chunk in manager.all) { Object.DestroyImmediate(chunk); }
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanBatchUpdateChunks()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>(chunk_prefab_path);
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>(world_prefab_path);

            Vector3Int size = new Vector3Int(2,2,2);
            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.size = size;
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);

            manager.Load(size, Vector3Int.zero);
            foreach (Chunk chunk in manager.all) { Assert.AreEqual(false, chunk.stale); }

            manager.Load(size, Vector3Int.zero);
            foreach (Chunk chunk in manager.all) { Assert.AreEqual(true, chunk.stale); }

            // clean up
            world.data.Dispose();
            foreach (Chunk chunk in manager.all) { Object.DestroyImmediate(chunk); }
            Object.DestroyImmediate(world);
        }
    }
}
