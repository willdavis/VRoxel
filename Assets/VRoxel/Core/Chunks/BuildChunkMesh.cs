using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using System;

namespace VRoxel.Core.Chunks
{
    /// <summary>
    /// Builds the vertices, triangles, and uvs for a Chunks mesh
    /// </summary>
    [BurstCompile]
    public struct BuildChunkMesh : IJob
    {
        public int3 chunkOffset;
        public int3 chunkSize;
        public int3 worldSize;
        public float worldScale;
        public float textureScale;

        /// <summary>
        /// Flags if chunk faces on the edge of the world should render
        /// </summary>
        public bool renderWorldEdges;

        /// <summary>
        /// a 2D reference array to the 6 faces of a cube and their vertices
        /// </summary>
        [ReadOnly] public NativeArray<int> cubeFaces;

        /// <summary>
        /// a reference to the 8 unit vectors that define the vertices of a cube
        /// </summary>
        [ReadOnly] public NativeArray<float3> cubeVertices;

        /// <summary>
        /// a reference to the unit vector directions
        /// </summary>
        [ReadOnly] public NativeArray<int3> directions;

        /// <summary>
        /// a reference to each blocks rendering settings
        /// </summary>
        [ReadOnly] public NativeArray<Block> blocks;

        /// <summary>
        /// the block indexes for each voxel in the chunk
        /// </summary>
        [ReadOnly] public NativeArray<byte> voxels;
        [ReadOnly] public NativeArray<byte> voxelsUp;
        [ReadOnly] public NativeArray<byte> voxelsDown;
        [ReadOnly] public NativeArray<byte> voxelsNorth;
        [ReadOnly] public NativeArray<byte> voxelsSouth;
        [ReadOnly] public NativeArray<byte> voxelsEast;
        [ReadOnly] public NativeArray<byte> voxelsWest;

        /// <summary>
        /// the vertices that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<Vector3> vertices;

        /// <summary>
        /// the triangles that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<int> triangles;

        /// <summary>
        /// the UV coordinates that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<Vector2> uvs;

        private int faceCount;
        private int blockCount;
        private float halfScale;

        /// <summary>
        /// Iterate over each voxel in the chunk
        /// and add mesh vertices, triangles, and uvs
        /// </summary>
        public void Execute()
        {
            Initialize();

            int3 grid = int3.zero;
            for (int x = 0; x < chunkSize.x; x++)
            {
                grid.x = x;
                for (int z = 0; z < chunkSize.z; z++)
                {
                    grid.z = z;
                    for (int y = 0; y < chunkSize.y; y++)
                    {
                        grid.y = y;
                        BuildCube(grid);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the job for another run
        /// </summary>
        public void Initialize()
        {
            halfScale = worldScale / 2f;
            blockCount = blocks.Length;
            faceCount = 0;

            if (triangles.IsCreated)
                triangles.Clear();

            if (vertices.IsCreated)
                vertices.Clear();

            if (uvs.IsCreated)
                uvs.Clear();
        }

        /// <summary>
        /// Calculates the position of a cube and adds
        /// all visible faces to the voxel mesh
        /// </summary>
        public void BuildCube(int3 grid)
        {
            Block block = new Block();
            float3 localPos = float3.zero;
            byte voxel = voxels[Flatten(grid)];

            // skip if the block is not renderable
            if (!TryGetBlock(voxel, ref block)) { return; }
            if (!block.collidable) { return; }

            // calculate the local scene position for this voxel
            localPos.x = (float)grid.x * worldScale;
            localPos.y = (float)grid.y * worldScale;
            localPos.z = (float)grid.z * worldScale;

            // offset the position to center it on the grid coordinate
            localPos.x -= 0.5f * ((float)chunkSize.x - 1f) * worldScale;
            localPos.y -= 0.5f * ((float)chunkSize.y - 1f) * worldScale;
            localPos.z -= 0.5f * ((float)chunkSize.z - 1f) * worldScale;

            // check visibility of each adjacent block and add faces
            for (int i = 0; i < 6; i++)
            {
                AddFace(i, block, grid, localPos);
            }
        }

        /// <summary>
        /// Adds vertices, triangles, and uvs for the face of a cube
        /// at the desired local position in a Chunk
        /// </summary>
        public void AddFace(int i, Block block, int3 grid, float3 localPos)
        {
            byte neighbor = 0;
            Block nextBlock = new Block();
            int3 next = grid + directions[i];

            // check if the adjacent block is out of bounds
            // and if there is block rendering data for it
            bool hasNextBlock = TryGetVoxel(next, ref neighbor);
            bool hasValidBlock = hasNextBlock ? TryGetBlock(neighbor, ref nextBlock) : false;

            // skip if there is no adjacent block
            // this occurs at the edges of the world
            if (!hasNextBlock && !renderWorldEdges) { return; }

            // skip if the adjacent block is collidable
            // this step culls the non-visible faces of the cube
            if (hasValidBlock && nextBlock.collidable) { return; }

            // render the face of the cube
            AddFaceVertices(i, localPos);
            AddFaceUV(i, block);
            AddFaceTriangles();
        }

        /// <summary>
        /// Adds 4 vertices to define the face for a cube
        /// centered at a local position in the Chunk
        /// </summary>
        public void AddFaceVertices(int dir, float3 localPos)
        {
            int index = 0;
            float3 vertex = float3.zero;
            for (int f = 0; f < 4; f++)
            {
                index = cubeFaces[(dir * 4) + f];
                vertex = cubeVertices[index];
                vertex *= halfScale;
                vertex += localPos;
                vertices.Add(vertex);
            }
        }

        /// <summary>
        /// Adds 4 UV coordinates to texture the cube face
        /// </summary>
        public void AddFaceUV(int dir, Block block)
        {
            float2 faceUV = float2.zero;
            float2 texture = float2.zero;

            switch (dir)
            {
                case 0: // Top
                    texture = block.texturesUp;
                    break;
                case 1: // Bottom
                    texture = block.texturesDown;
                    break;
                case 2: // North (Front)
                    texture = block.texturesFront;
                    break;
                case 3: // East (Right)
                    texture = block.texturesRight;
                    break;
                case 4: // South (Back)
                    texture = block.texturesBack;
                    break;
                case 5: // West (Left)
                    texture = block.texturesLeft;
                    break;
                default:
                    return;
            }

            faceUV.x = textureScale * texture.x + textureScale;
            faceUV.y = textureScale * texture.y;
            uvs.Add(faceUV);

            faceUV.x = textureScale * texture.x + textureScale;
            faceUV.y = textureScale * texture.y + textureScale;
            uvs.Add(faceUV);

            faceUV.x = textureScale * texture.x;
            faceUV.y = textureScale * texture.y + textureScale;
            uvs.Add(faceUV);

            faceUV.x = textureScale * texture.x;
            faceUV.y = textureScale * texture.y;
            uvs.Add(faceUV);
        }

        /// <summary>
        /// Adds 2 triangles to form a quad for the cube face
        /// </summary>
        public void AddFaceTriangles()
        {
            triangles.Add(faceCount * 4);      // 1
            triangles.Add(faceCount * 4 + 1);  // 2
            triangles.Add(faceCount * 4 + 2);  // 3
            triangles.Add(faceCount * 4);      // 1
            triangles.Add(faceCount * 4 + 2);  // 3
            triangles.Add(faceCount * 4 + 3);  // 4
            faceCount++;
        }

        /// <summary>
        /// Attempts to fetch a block using a voxel and
        /// returns false if no block can be found
        /// </summary>
        public bool TryGetBlock(byte id, ref Block block)
        {
            if (id < 0 || id >= blockCount)
                return false;

            block = blocks[id];
            return true;
        }

        /// <summary>
        /// Attempts to fetch a voxel from the chunk and
        /// returns false if the grid coordinate is not valid
        /// </summary>
        public bool TryGetVoxel(int3 grid, ref byte voxel)
        {
            // early exit if the position is out of bounds
            if (OutOfWorld(grid + chunkOffset)) { return false; }

            // check if the position is outside the chunk
            // if so, fetch the voxel from the adjacent chunk
            if (OutOfChunk(grid))
                voxel = GetFromNeighbor(grid);
            else
                voxel = voxels[Flatten(grid)];

            return true;
        }

        /// <summary>
        /// Fetch a voxel from a neighboring chunk
        /// </summary>
        public byte GetFromNeighbor(int3 grid)
        {
            int3 localPos = grid;
            NativeArray<byte> data = voxels;

            if (grid.x < 0) // Left (West)
            {
                data = voxelsWest;
                localPos.x = chunkSize.x - 1;
            }
            else if (grid.z < 0) // Back (South)
            {
                data = voxelsSouth;
                localPos.z = chunkSize.z - 1;
            }
            else if (grid.y < 0) // Down
            {
                data = voxelsDown;
                localPos.y = chunkSize.y - 1;
            }
            else if (grid.x == chunkSize.x) // Right (East)
            {
                data = voxelsEast;
                localPos.x = 0;
            }
            else if (grid.z == chunkSize.z) // Front (North)
            {
                data = voxelsNorth;
                localPos.z = 0;
            }
            else if (grid.y == chunkSize.y) // Up
            {
                data = voxelsUp;
                localPos.y = 0;
            }

            return data[Flatten(localPos)];
        }

        /// <summary>
        /// Test if a point is outside of the Chunk's boundary
        /// </summary>
        public bool OutOfChunk(int3 grid)
        {
            if (grid.x < 0 || grid.x >= chunkSize.x) { return true; }
            if (grid.y < 0 || grid.y >= chunkSize.y) { return true; }
            if (grid.z < 0 || grid.z >= chunkSize.z) { return true; }
            return false;
        }

        /// <summary>
        /// Test if a point is outside of the World's boundary
        /// </summary>
        public bool OutOfWorld(int3 grid)
        {
            if (grid.x < 0 || grid.x >= worldSize.x) { return true; }
            if (grid.y < 0 || grid.y >= worldSize.y) { return true; }
            if (grid.z < 0 || grid.z >= worldSize.z) { return true; }
            return false;
        }

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) point
        /// </summary>
        /// <param name="point">A local position inside the Chunk</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * chunkSize.y * chunkSize.z) + (point.y * chunkSize.z) + point.z;
        }
    }

    /// <summary>
    /// Defines the rendering properties of a voxel block
    /// </summary>
    [Serializable]
    public struct Block
    {
        public bool collidable;
        public float2 texturesUp;
        public float2 texturesDown;
        public float2 texturesFront;
        public float2 texturesBack;
        public float2 texturesLeft;
        public float2 texturesRight;
    }
}
