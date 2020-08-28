using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using VRoxel.Core;
using VRoxel.Core.Data;

namespace CoreSpecs
{
    public class MeshGeneratorSpec
    {
        [Test]
        public void CanBuildMesh()
        {
            Mesh mesh = new Mesh();
            BlockManager manager = new BlockManager();
            VoxelGrid data = new VoxelGrid(Vector3Int.one);
            MeshGenerator generator = new MeshGenerator(data, manager, 1f);

            BlockConfiguration air = new BlockConfiguration();
            BlockConfiguration block = new BlockConfiguration();

            // setup a block to be rendered
            manager.blocks.Add(air);
            manager.blocks.Add(block);
            block.texture = Vector2.one;
            data.Set(Vector3Int.zero, 1);

            // generate 1 cube
            generator.BuildMesh(Vector3Int.one, Vector3Int.zero, ref mesh);

            // confirm 1 cube was generated
            Assert.AreEqual(24, mesh.vertices.GetLength(0));  // expect 4x6 vertices
            Assert.AreEqual(36, mesh.triangles.GetLength(0)); // expect 6x6 triangles
            Assert.AreEqual(24, mesh.uv.GetLength(0));        // expect 4x6 uv coordinates

            data.Dispose();
        }
    }
}
