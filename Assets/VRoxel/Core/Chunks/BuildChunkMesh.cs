using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Core.Chunks
{
    /// <summary>
    /// Builds the vertices, triangles, and uvs for a Chunks mesh
    /// </summary>
    [BurstCompile]
    public struct BuildChunkMesh : IJob
    {
        public int3 size;
        public float worldScale;
        public float textureScale;

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
        /// the block indexes for each voxel in the world.
        /// </summary>
        [ReadOnly] public NativeArray<byte> voxels;

        /// <summary>
        /// the vertices that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<float3> vertices;

        /// <summary>
        /// the triangles that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<int> triangles;

        /// <summary>
        /// the UV coordinates that will be used for the Chunks mesh
        /// </summary>
        [WriteOnly] public NativeList<float2> uvs;

        private int faceCount;
        private int blockCount;
        private float halfScale;

        /// <summary>
        /// Iterate over each voxel in the chunk
        /// and add mesh vertices, triangles, and uvs
        /// </summary>
        public void Execute()
        {
            int3 grid = int3.zero;
            blockCount = blocks.Length;
            halfScale = worldScale / 2f;

            for (int x = 0; x < size.x; x++)
            {
                grid.x = x;
                for (int z = 0; z < size.z; z++)
                {
                    grid.z = z;
                    for (int y = 0; y < size.y; y++)
                    {
                        grid.y = y;
                        BuildCube(grid);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the position of a cube and adds
        /// the cubes faces to the voxel mesh
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
            localPos.x -= 0.5f * ((float)size.x - 1f) * worldScale;
            localPos.y -= 0.5f * ((float)size.y - 1f) * worldScale;
            localPos.z -= 0.5f * ((float)size.z - 1f) * worldScale;

            // check each neighbor for a non-collidable block
            // add a cube face to the mesh if one is found
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
            bool hasNext = TryGetVoxel(next, ref neighbor);
            bool hasBlock = hasNext ? TryGetBlock(neighbor, ref nextBlock) : false;

            // skip if the adjacent block is out of bounds
            if (!hasNext) { return; }

            // skip if the adjacent block is collidable
            if ( hasBlock && nextBlock.collidable ) { return; }

            // render a face for the cube
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
            faceUV.x = textureScale * block.textures[dir].x + textureScale;
            faceUV.y = textureScale * block.textures[dir].y;
            uvs.Add(faceUV);

            faceUV.x = textureScale * block.textures[dir].x + textureScale;
            faceUV.y = textureScale * block.textures[dir].y + textureScale;
            uvs.Add(faceUV);

            faceUV.x = textureScale * block.textures[dir].x;
            faceUV.y = textureScale * block.textures[dir].y + textureScale;
            uvs.Add(faceUV);

            faceUV.x = textureScale * block.textures[dir].x;
            faceUV.y = textureScale * block.textures[dir].y;
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
        /// Attempts to fetch a voxel from the world and
        /// returns false if the grid coordinate is out of bounds
        /// </summary>
        public bool TryGetVoxel(int3 grid, ref byte voxel)
        {
            if (OutOfBounds(grid)) { return false; }

            voxel = voxels[Flatten(grid)];
            return true;
        }

        /// <summary>
        /// Test if a point is outside the Chunks voxel grid
        /// </summary>
        public bool OutOfBounds(int3 grid)
        {
            if (grid.x < 0 || grid.x >= size.x) { return true; }
            if (grid.y < 0 || grid.y >= size.y) { return true; }
            if (grid.z < 0 || grid.z >= size.z) { return true; }
            return false;
        }

        /// <summary>
        /// Calculate an array index from a int3 (Vector3Int) point
        /// </summary>
        /// <param name="point">A point in the flow field</param>
        public int Flatten(int3 point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }
    }

    /// <summary>
    /// Defines the rendering properties of a voxel block
    /// </summary>
    public struct Block
    {
        public bool collidable;
        public float2[] textures;
    }
}
