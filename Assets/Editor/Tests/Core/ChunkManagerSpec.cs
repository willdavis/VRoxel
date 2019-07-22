﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace Tests
{
    public class ChunkManagerSpec
    {
        [Test]
        public void CanContainIndexes()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");
            ChunkManager manager = new ChunkManager(prefab_world, prefab_chunk);

            Assert.AreEqual(true, manager.Contains(Vector3Int.zero));
            Assert.AreEqual(false, manager.Contains(Vector3Int.one));
        }

        [Test]
        public void CanCreateChunks()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

            World world = UnityEngine.Object.Instantiate(prefab_world, Vector3.zero, Quaternion.identity) as World;
            world.Initialize();

            ChunkManager manager = new ChunkManager(world, prefab_chunk);
            Assert.AreEqual(null, manager.Get(Vector3Int.one));  // out of bounds
            Assert.AreEqual(null, manager.Get(Vector3Int.zero)); // no chunk is present

            Chunk chunk = manager.Create(Vector3Int.zero);
            Assert.AreSame(chunk, manager.Get(Vector3Int.zero)); // confirm chunk was cached

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanUpdateChunk()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

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

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(world);
        }

        [Test]
        public void CanUpdateChunkNeighbors()
        {
            Chunk prefab_chunk = AssetDatabase.LoadAssetAtPath<Chunk>("Assets/VRoxel/Prefabs/Chunk.prefab");
            World prefab_world = AssetDatabase.LoadAssetAtPath<World>("Assets/VRoxel/Prefabs/World.prefab");

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

            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(control);
            Object.DestroyImmediate(neighbor);
            Object.DestroyImmediate(world);
        }
    }
}