using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using VRoxel.Core;

namespace CoreSpecs
{
    public class MeshGeneratorSpec
    {
        [Test]
        public void CanBuildMesh()
        {
            Mesh mesh = new Mesh();
            Block block = new Block();
            BlockManager manager = new BlockManager();
            VoxelGrid data = new VoxelGrid(Vector3Int.one);
            MeshGenerator generator = new MeshGenerator(data, manager, 1f);

            // setup a block to be rendered
            data.Set(Vector3Int.zero, 1);
            manager.library.Add(1, block);

            block.textures.Add(Cube.Direction.Top, Vector2.zero);
            block.textures.Add(Cube.Direction.Bottom, Vector2.zero);
            block.textures.Add(Cube.Direction.North, Vector2.zero);
            block.textures.Add(Cube.Direction.East, Vector2.zero);
            block.textures.Add(Cube.Direction.South, Vector2.zero);
            block.textures.Add(Cube.Direction.West, Vector2.zero);

            // generate 1 cube
            generator.BuildMesh(Vector3Int.one, Vector3Int.zero, ref mesh);

            // confirm 1 cube was generated
            Assert.AreEqual(24, mesh.vertices.GetLength(0));  // expect 4x6 vertices
            Assert.AreEqual(36, mesh.triangles.GetLength(0)); // expect 6x6 triangles
            Assert.AreEqual(24, mesh.uv.GetLength(0));        // expect 4x6 uv coordinates
        }
    }
}
