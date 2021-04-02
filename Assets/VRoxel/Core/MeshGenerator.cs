using VRoxel.Core.Chunks;
using VRoxel.Core.Data;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
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

        public NativeArray<Block> blockLibrary
        {
            get { return m_blocks; }
        }

        public MeshGenerator(World world)
        {
            m_chunkConfig = world.chunkManager.configuration;
            m_blockManager = world.blockManager;
            m_world = world;

            /// create lookup tables for cube rendering data
            int size1D = m_chunkConfig.size.x * m_chunkConfig.size.y * m_chunkConfig.size.z;
            m_emptyChunk = new NativeArray<byte>(size1D, Allocator.Persistent);
            m_cubeFaces = new NativeArray<int>(Cube.Faces.Length, Allocator.Persistent);
            m_cubeVertices = new NativeArray<float3>(Cube.Vectors.Length, Allocator.Persistent);
            m_directions = new NativeArray<int3>(Cube.Directions.Length, Allocator.Persistent);
            m_blocks = new NativeArray<Block>(m_blockManager.blocks.Count, Allocator.Persistent);

            m_cubeFaces.CopyFrom(Cube.Faces);
            m_cubeVertices.CopyFrom(Cube.Vectors);
            m_directions.CopyFrom(Cube.Directions);

            /// convert block configurations to a block structure
            /// of material and texture data for rendering
            for (int i = 0; i < m_blockManager.blocks.Count; i++)
            {
                BlockConfiguration blockConfig = m_blockManager.blocks[i];
                m_blocks[i] = new Block()
                {
                    editable = blockConfig.editable,
                    collidable = blockConfig.collidable,
                    texturesUp = blockConfig.textureUp,
                    texturesDown = blockConfig.textureDown,
                    texturesBack = blockConfig.textureBack,
                    texturesFront = blockConfig.textureFront,
                    texturesRight = blockConfig.textureRight,
                    texturesLeft = blockConfig.textureLeft,
                };
            }
        }

        /// <summary>
        /// Disposes all unmanaged memory
        /// </summary>
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
        /// Schedules a background job to build the vertices, 
        /// triangles, and uv data for a chunk's mesh
        /// </summary>
        /// <param name="chunk">The Chunk to generate a mesh for</param>
        /// <param name="job">References the chunk's cached background job</param>
        /// <param name="verts">References the chunk's cached vertices</param>
        /// <param name="tris">References the chunk's cached triangles</param>
        /// <param name="uv">References the chunk's cached uv data</param>
        public JobHandle BuildMesh(Chunk chunk,
            ref BuildChunkMesh job, ref NativeList<Vector3> verts,
            ref NativeList<int> tris, ref NativeList<Vector2> uv,
            JobHandle dependsOn = default)
        {
            job.chunkSize = new int3(chunk.size.x, chunk.size.y, chunk.size.z);
            job.chunkOffset = new int3(chunk.offset.x, chunk.offset.y, chunk.offset.z);
            job.worldSize = new int3(m_world.size.x, m_world.size.y, m_world.size.z);
            job.renderWorldEdges = m_world.renderWorldEdges;
            job.worldScale = m_world.scale;

            // configure texture settings for the mesh
            job.textureScale = chunk.configuration.textureScale;
            job.textureOffset = chunk.configuration.textureOffset;

            job.cubeFaces = m_cubeFaces;
            job.cubeVertices = m_cubeVertices;
            job.directions = m_directions;
            job.blocks = m_blocks;

            job.vertices = verts;
            job.triangles = tris;
            job.uvs = uv;

            job.voxels = chunk.voxels;
            if (chunk.neighbors.up)
                job.voxelsUp = chunk.neighbors.up.voxels;
            else { job.voxelsUp = m_emptyChunk; }

            if (chunk.neighbors.down)
                job.voxelsDown = chunk.neighbors.down.voxels;
            else { job.voxelsDown = m_emptyChunk; }

            if (chunk.neighbors.north)
                job.voxelsNorth = chunk.neighbors.north.voxels;
            else { job.voxelsNorth = m_emptyChunk; }

            if (chunk.neighbors.south)
                job.voxelsSouth = chunk.neighbors.south.voxels;
            else { job.voxelsSouth = m_emptyChunk; }

            if (chunk.neighbors.east)
                job.voxelsEast = chunk.neighbors.east.voxels;
            else { job.voxelsEast = m_emptyChunk; }

            if (chunk.neighbors.west)
                job.voxelsWest = chunk.neighbors.west.voxels;
            else { job.voxelsWest = m_emptyChunk; }

            return job.Schedule(dependsOn);
        }
    }
}
