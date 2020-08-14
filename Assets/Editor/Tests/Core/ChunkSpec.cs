using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using VRoxel.Core;
using VRoxel.Core.Data;

namespace CoreSpecs
{
    public class ChunkSpec
    {
        [Test]
        public void CanReadData()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshCollider>();
            obj.AddComponent<MeshFilter>();

            Chunk chunk = obj.AddComponent<Chunk>();
            ChunkConfiguration config = ScriptableObject
                .CreateInstance("ChunkConfiguration") as ChunkConfiguration;

            chunk.configuration = config;
            chunk.Initialize();

            Assert.AreEqual(0, chunk.Read(Vector3Int.zero));
            Assert.AreEqual(255, chunk.Read(Vector3Int.one));
            chunk.Dispose();
        }

        [Test]
        public void CanWriteData()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshCollider>();
            obj.AddComponent<MeshFilter>();

            Chunk chunk = obj.AddComponent<Chunk>();
            ChunkConfiguration config = ScriptableObject
                .CreateInstance("ChunkConfiguration") as ChunkConfiguration;

            chunk.configuration = config;
            chunk.Initialize();

            chunk.Write(Vector3Int.zero, 10);
            Assert.AreEqual(10, chunk.Read(Vector3Int.zero));
            chunk.Dispose();
        }
    }
}
