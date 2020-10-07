using System.Collections.Generic;
using VRoxel.Core.Chunks;
using VRoxel.Core.Data;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VRoxel.Core
{
    public class MeshGenerator
    {
        private World m_world;
        private BlockManager m_blockManager;
        private ChunkConfiguration m_chunkConfig;

        private NativeArray<byte> m_emptyChunk;
        private NativeArray<int> m_cubeFaces;
        private NativeArray<float3> m_cubeVertices;
        private NativeArray<int3> m_directions;
        private NativeArray<Block> m_blocks;

        private List<Vector3> _meshVert = new List<Vector3>();
        private List<int> _meshTri = new List<int>();
        private List<Vector2> _meshUV = new List<Vector2>();
        private Vector3[] _face = new Vector3[4];
        private Vector2[] _faceUV = new Vector2[4];
        private int _faceCount = 0;
        private float _halfScale;
        private float _scale;

        public MeshGenerator(World world, BlockManager manager, ChunkConfiguration config)
        {
            m_world = world;
            m_blockManager = manager;
            m_chunkConfig = config;

            _scale = config.sizeScale;
            _halfScale = config.sizeScale * 0.5f;

            int size1D = config.size.x * config.size.y * config.size.z;
            m_emptyChunk = new NativeArray<byte>(size1D, Allocator.Persistent);
            m_cubeFaces = new NativeArray<int>(Cube.Faces.Length, Allocator.Persistent);
            m_cubeVertices = new NativeArray<float3>(Cube.Vectors.Length, Allocator.Persistent);
            m_directions = new NativeArray<int3>(Cube.Directions.Length, Allocator.Persistent);
            m_blocks = new NativeArray<Block>(manager.blocks.Count, Allocator.Persistent);

            for (int i = 0; i < manager.blocks.Count; i++)
            {
                BlockConfiguration blockConfig = manager.blocks[i];
                m_blocks[i] = new Block()
                {
                    collidable = blockConfig.collidable,
                    texturesTop = blockConfig.texture,
                    texturesBottom = blockConfig.texture,
                    texturesBack = blockConfig.texture,
                    texturesFront = blockConfig.texture,
                    texturesLeft = blockConfig.texture,
                    texturesRight = blockConfig.texture,
                };
            }

            m_cubeFaces.CopyFrom(Cube.Faces);
            m_cubeVertices.CopyFrom(Cube.Vectors);
            m_directions.CopyFrom(Cube.Directions);
        }

        public void Dispose()
        {
            if (m_emptyChunk.IsCreated)
                m_emptyChunk.Dispose();

            if (m_cubeFaces.IsCreated)
                m_cubeFaces.Dispose();

            if (m_cubeVertices.IsCreated)
                m_cubeVertices.Dispose();

            if (m_directions.IsCreated)
                m_directions.Dispose();

            if (m_blocks.IsCreated)
                m_blocks.Dispose();
        }

        /// <summary>
        /// Updates the mesh for a section of the voxel grid
        /// </summary>
        /// <param name="size">The size of the voxel mesh to render</param>
        /// <param name="offset">The offset for the voxel data</param>
        /// <param name="mesh">The referenced Mesh to update</param>
        public void BuildMesh(Vector3Int size, Vector3Int offset, ref Mesh mesh)
        {
            Vector3Int voxel = Vector3Int.zero;
            Vector3 position = Vector3.zero;
            BlockConfiguration block;
            int blockCount;
            byte index;

            mesh.Clear();
            blockCount = m_blockManager.blocks.Count;

            // generate faces (adjacent to air) for all solid blocks
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        voxel.x = x;
                        voxel.y = y;
                        voxel.z = z;
                        voxel += offset;

                        index = m_world.data.Get(voxel);
                        if (index == 0) { continue; }   // skip if the block is air

                        position.x = (float)x * _scale;
                        position.y = (float)y * _scale;
                        position.z = (float)z * _scale;

                        position.x -= 0.5f * ((float)size.x - 1f) * _scale;
                        position.y -= 0.5f * ((float)size.y - 1f) * _scale;
                        position.z -= 0.5f * ((float)size.z - 1f) * _scale;

                        // if no block data is present, exit and do not render the mesh
                        if(index >= blockCount)
                        {
                            Debug.LogAssertion("Chunk failed to render: no block found with index:" + index);
                            ClearCache();
                            return;
                        }

                        block = m_blockManager.blocks[index];
                        if (m_world.data.Get(voxel + Direction3Int.Up) == 0)    { AddFace(position, block, Cube.Direction.Top);    }
                        if (m_world.data.Get(voxel + Direction3Int.Down) == 0)  { AddFace(position, block, Cube.Direction.Bottom); }
                        if (m_world.data.Get(voxel + Direction3Int.North) == 0) { AddFace(position, block, Cube.Direction.North);  }
                        if (m_world.data.Get(voxel + Direction3Int.East) == 0)  { AddFace(position, block, Cube.Direction.East);   }
                        if (m_world.data.Get(voxel + Direction3Int.South) == 0) { AddFace(position, block, Cube.Direction.South);  }
                        if (m_world.data.Get(voxel + Direction3Int.West) == 0)  { AddFace(position, block, Cube.Direction.West);   }
                    }
                }
            }

            mesh.vertices = _meshVert.ToArray();
            mesh.triangles = _meshTri.ToArray();
            mesh.uv = _meshUV.ToArray();
            mesh.RecalculateNormals();
            ClearCache();
        }

        private void ClearCache()
        {
            _meshVert.Clear();
            _meshTri.Clear();
            _meshUV.Clear();
            _faceCount = 0;
        }

        /// <summary>
        /// Add cube face vertices, triangles, and UVs to the cache
        /// </summary>
        /// <param name="position">The position of the cube</param>
        /// <param name="block">The block data for the cube</param>
        /// <param name="dir">The direction of the cube to render</param>
        private void AddFace(Vector3 position, BlockConfiguration block, Cube.Direction dir)
        {
            float tScale = m_blockManager.textureAtlas.scale;
            Vector2 texture = block.texture;

            // add vertices for the face
            Cube.Face((int)dir, position, _halfScale, ref _face);
            _meshVert.AddRange(_face);

            // add uv coordinates for the face
            _faceUV[0].x = tScale * texture.x + tScale;
            _faceUV[0].y = tScale * texture.y;
            _faceUV[1].x = tScale * texture.x + tScale;
            _faceUV[1].y = tScale * texture.y + tScale;
            _faceUV[2].x = tScale * texture.x;
            _faceUV[2].y = tScale * texture.y + tScale;
            _faceUV[3].x = tScale * texture.x;
            _faceUV[3].y = tScale * texture.y;
            _meshUV.AddRange(_faceUV);

            // add triangles for the face
            _meshTri.Add(_faceCount * 4);      // 1
            _meshTri.Add(_faceCount * 4 + 1);  // 2
            _meshTri.Add(_faceCount * 4 + 2);  // 3
            _meshTri.Add(_faceCount * 4);      // 1
            _meshTri.Add(_faceCount * 4 + 2);  // 3
            _meshTri.Add(_faceCount * 4 + 3);  // 4
            _faceCount++;
        }
    }
}
