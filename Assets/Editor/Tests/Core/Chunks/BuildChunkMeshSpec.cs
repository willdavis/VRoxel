using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;
using VRoxel.Core.Chunks;
using VRoxel.Core;

namespace CoreChunksSpecs
{
    public class BuildChunkMeshSpec
    {
        [Test]
        public void CanFlattenGridPositions()
        {
            BuildChunkMesh job = new BuildChunkMesh();
            job.chunkSize = new int3(3,3,3);

            Assert.AreEqual(0, job.Flatten(int3.zero));
            Assert.AreEqual(13, job.Flatten(new int3(1,1,1)));
            Assert.AreEqual(26, job.Flatten(new int3(2,2,2)));
        }

        [Test]
        public void CanCheckForOutOfWorld()
        {
            BuildChunkMesh job = new BuildChunkMesh();
            job.worldSize = new int3(3,3,3);

            Assert.AreEqual(false, job.OutOfWorld(int3.zero));
            Assert.AreEqual(false, job.OutOfWorld(new int3(2,2,2)));
            Assert.AreEqual(true, job.OutOfWorld(new int3(-1,-1,-1)));
            Assert.AreEqual(true, job.OutOfWorld(new int3(3,3,3)));
        }

        [Test]
        public void CanCheckForOutOfChunk()
        {
            BuildChunkMesh job = new BuildChunkMesh();
            job.chunkSize = new int3(3,3,3);

            Assert.AreEqual(false, job.OutOfChunk(int3.zero));
            Assert.AreEqual(false, job.OutOfChunk(new int3(2,2,2)));
            Assert.AreEqual(true, job.OutOfChunk(new int3(-1,-1,-1)));
            Assert.AreEqual(true, job.OutOfChunk(new int3(3,3,3)));
        }

        [Test]
        public void CanGetVoxelsFromTheChunk()
        {
            byte voxel = 0;
            int3 size = new int3(3,3,3);
            int flatSize = size.x * size.y * size.z;
            NativeArray<byte> voxels = new NativeArray<byte>(
                flatSize, Allocator.Persistent);

            BuildChunkMesh job = new BuildChunkMesh();
            job.chunkOffset = int3.zero;
            job.chunkSize = size;
            job.worldSize = size;
            job.voxels = voxels;

            /// set voxel data
            voxels[13] = 1;
            voxels[26] = 1;

            // returns false when the position is out of bounds
            Assert.AreEqual(false, job.TryGetVoxel(new int3(3,3,3), ref voxel));

            // returns the voxel at the position
            Assert.AreEqual(true, job.TryGetVoxel(int3.zero, ref voxel));
            Assert.AreEqual(0, voxel);

            Assert.AreEqual(true, job.TryGetVoxel(new int3(1,1,1), ref voxel));
            Assert.AreEqual(1, voxel);

            Assert.AreEqual(true, job.TryGetVoxel(new int3(2,2,2), ref voxel));
            Assert.AreEqual(1, voxel);

            voxels.Dispose();
        }

        [Test]
        public void CanGetVoxelsFromAdjacentChunks()
        {
            byte voxel = 0;
            int3 chunkSize = new int3(3,3,3);
            int3 worldSize = new int3(9,9,9);
            int flatSize = chunkSize.x * chunkSize.y * chunkSize.z;

            NativeArray<byte> voxels = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsTop = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsBottom = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsNorth = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsEast = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsSouth = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> voxelsWest = new NativeArray<byte>(
                flatSize, Allocator.Persistent);
            NativeArray<byte> emptyChunk = new NativeArray<byte>(
                flatSize, Allocator.Persistent);

            BuildChunkMesh job = new BuildChunkMesh();
            job.chunkOffset = new int3(3,3,3);
            job.chunkSize = chunkSize;
            job.worldSize = worldSize;
            job.voxelsTop = voxelsTop;
            job.voxelsBottom = voxelsBottom;
            job.voxelsNorth = voxelsNorth;
            job.voxelsEast = voxelsEast;
            job.voxelsSouth = voxelsSouth;
            job.voxelsWest = voxelsWest;
            job.voxels = voxels;

            /// set voxel data
            voxelsTop[0]    = 6;
            voxelsBottom[6] = 5;
            voxelsNorth[0]  = 4;
            voxelsEast[0]   = 3;
            voxelsSouth[2]  = 2;
            voxelsWest[18]  = 1;

            // returns false when the position is out of bounds
            Assert.AreEqual(false, job.TryGetVoxel(new int3(9,9,9), ref voxel));

            // returns the voxel from the top chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(0,3,0), ref voxel));
            Assert.AreEqual(6, voxel);

            // returns the voxel from the bottom chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(0,-1,0), ref voxel));
            Assert.AreEqual(5, voxel);

            // returns the voxel from the north chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(0,0,3), ref voxel));
            Assert.AreEqual(4, voxel);

            // returns the voxel from the east chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(3,0,0), ref voxel));
            Assert.AreEqual(3, voxel);

            // returns the voxel from the south chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(0,0,-1), ref voxel));
            Assert.AreEqual(2, voxel);

            // returns the voxel from the west chunk
            Assert.AreEqual(true, job.TryGetVoxel(new int3(-1,0,0), ref voxel));
            Assert.AreEqual(1, voxel);

            // returns 0 when the neighbor chunk is empty
            job.voxelsWest = emptyChunk;
            Assert.AreEqual(true, job.TryGetVoxel(new int3(-1,0,0), ref voxel));
            Assert.AreEqual(0, voxel);

            voxels.Dispose();
            voxelsTop.Dispose();
            voxelsBottom.Dispose();
            voxelsNorth.Dispose();
            voxelsEast.Dispose();
            voxelsSouth.Dispose();
            voxelsWest.Dispose();
            emptyChunk.Dispose();
        }

        [Test]
        public void CanGetABlockFromAVoxel()
        {
            Block block = new Block();
            Block original = new Block();
            original.collidable = true;

            NativeArray<Block> blocks = new NativeArray<Block>(1, Allocator.Persistent);
            blocks[0] = original;

            BuildChunkMesh job = new BuildChunkMesh();
            job.blocks = blocks;
            job.Initialize();

            /// returns false if the index is out of bounds
            Assert.AreEqual(false, job.TryGetBlock(1, ref block));

            /// returns the block using the voxel index
            Assert.AreEqual(true, job.TryGetBlock(0, ref block));
            Assert.AreEqual(true, block.collidable);
            Assert.AreEqual(original, block);

            blocks.Dispose();
        }

        /// Tests for creating the Mesh

        [Test]
        public void CanAddTrianglesToTheMesh()
        {
            NativeList<int> triangles = new NativeList<int>(Allocator.Persistent);
            BuildChunkMesh job = new BuildChunkMesh();
            job.triangles = triangles;
            job.Initialize();

            Assert.AreEqual(0, job.triangles.Length);
            job.AddFaceTriangles();
            Assert.AreEqual(6, job.triangles.Length);

            triangles.Dispose();
        }

        [Test]
        public void CanAddVerticesToTheMesh()
        {
            NativeArray<float3> cubeVertices = new NativeArray<float3>(Cube.Vectors.Length, Allocator.Persistent);
            cubeVertices.CopyFrom(Cube.Vectors);

            NativeArray<int> cubeFaces = new NativeArray<int>(Cube.Faces.Length, Allocator.Persistent);
            cubeFaces.CopyFrom(Cube.Faces);

            NativeList<Vector3> vertices = new NativeList<Vector3>(Allocator.Persistent);
            BuildChunkMesh job = new BuildChunkMesh();
            job.cubeVertices = cubeVertices;
            job.cubeFaces = cubeFaces;
            job.vertices = vertices;
            job.Initialize();

            Assert.AreEqual(0, job.vertices.Length);

            job.AddFaceVertices(0, float3.zero);
            Assert.AreEqual(4, job.vertices.Length);

            job.AddFaceVertices(1, float3.zero);
            Assert.AreEqual(8, job.vertices.Length);

            vertices.Dispose();
            cubeFaces.Dispose();
            cubeVertices.Dispose();
        }

        [Test]
        public void CanAddUVsToTheMesh()
        {
            Block block = new Block();

            NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Persistent);
            BuildChunkMesh job = new BuildChunkMesh();
            job.textureScale = 0.5f;
            job.uvs = uvs;

            job.AddFaceUV(0, block);
            Assert.AreEqual(4, job.uvs.Length);

            uvs.Dispose();
        }

        [Test]
        public void CanCreateFacesOfACube()
        {
            Block block = new Block();

            NativeArray<Block> blocks = new NativeArray<Block>(1, Allocator.Persistent);
            NativeArray<float3> cubeVertices = new NativeArray<float3>(Cube.Vectors.Length, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(Cube.Directions.Length, Allocator.Persistent);
            NativeArray<int> cubeFaces = new NativeArray<int>(Cube.Faces.Length, Allocator.Persistent);

            directions.CopyFrom(Cube.Directions);
            cubeVertices.CopyFrom(Cube.Vectors);
            cubeFaces.CopyFrom(Cube.Faces);

            int3 size = new int3(3,3,3);
            int flatSize = size.x * size.y * size.z;
            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

            NativeList<Vector3> vertices = new NativeList<Vector3>(Allocator.Persistent);
            NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Persistent);
            NativeList<int> triangles = new NativeList<int>(Allocator.Persistent);

            BuildChunkMesh job = new BuildChunkMesh();
            job.directions = directions;
            job.blocks = blocks;
            job.voxels = voxels;
            job.cubeVertices = cubeVertices;
            job.cubeFaces = cubeFaces;

            job.chunkSize = size;
            job.worldSize = size;
            job.worldScale = 1f;
            job.textureScale = 0.5f;
            job.chunkOffset = int3.zero;

            job.vertices = vertices;
            job.triangles = triangles;
            job.uvs = uvs;

            job.Initialize();
            job.AddFace(0, block, int3.zero, float3.zero);

            Assert.AreEqual(4, job.uvs.Length);
            Assert.AreEqual(4, job.vertices.Length);
            Assert.AreEqual(6, job.triangles.Length);

            uvs.Dispose();
            vertices.Dispose();
            triangles.Dispose();
            voxels.Dispose();

            cubeFaces.Dispose();
            cubeVertices.Dispose();
            directions.Dispose();
            blocks.Dispose();
        }

        [Test]
        public void CanCreateACube()
        {
            NativeArray<Block> blocks = new NativeArray<Block>(2, Allocator.Persistent);
            blocks[0] = new Block() { collidable = false }; // Air block
            blocks[1] = new Block() { collidable = true };  // Solid block

            NativeArray<float3> cubeVertices = new NativeArray<float3>(Cube.Vectors.Length, Allocator.Persistent);
            NativeArray<int3> directions = new NativeArray<int3>(Cube.Directions.Length, Allocator.Persistent);
            NativeArray<int> cubeFaces = new NativeArray<int>(Cube.Faces.Length, Allocator.Persistent);

            directions.CopyFrom(Cube.Directions);
            cubeVertices.CopyFrom(Cube.Vectors);
            cubeFaces.CopyFrom(Cube.Faces);

            int3 size = new int3(3,3,3);
            int flatSize = size.x * size.y * size.z;
            NativeArray<byte> voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);
            voxels[12] = 1; voxels[13] = 1; voxels[14] = 1; // cull 2 faces around voxels[13] (1,1,1)

            NativeList<Vector3> vertices = new NativeList<Vector3>(Allocator.Persistent);
            NativeList<Vector2> uvs = new NativeList<Vector2>(Allocator.Persistent);
            NativeList<int> triangles = new NativeList<int>(Allocator.Persistent);

            BuildChunkMesh job = new BuildChunkMesh();
            job.directions = directions;
            job.blocks = blocks;
            job.voxels = voxels;
            job.cubeVertices = cubeVertices;
            job.cubeFaces = cubeFaces;

            job.chunkSize = size;
            job.worldSize = size;
            job.worldScale = 1f;
            job.textureScale = 0.5f;
            job.chunkOffset = int3.zero;

            job.vertices = vertices;
            job.triangles = triangles;
            job.uvs = uvs;

            job.Initialize();
            job.BuildCube(new int3(1,1,1));

            Assert.AreEqual(16, job.uvs.Length);
            Assert.AreEqual(16, job.vertices.Length);
            Assert.AreEqual(24, job.triangles.Length);

            uvs.Dispose();
            vertices.Dispose();
            triangles.Dispose();
            voxels.Dispose();

            cubeFaces.Dispose();
            cubeVertices.Dispose();
            directions.Dispose();
            blocks.Dispose();
        }
    }
}
