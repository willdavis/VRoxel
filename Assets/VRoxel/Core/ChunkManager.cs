using System.Collections.Generic;
using VRoxel.Core.Data;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// A component to help manage the chunks of a voxel world
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        /// <summary>
        /// A reference to the voxel world
        /// </summary>
        public World world;

        /// <summary>
        /// The prefab to use when creating Chunks
        /// </summary>
        public Chunk chunkPrefab;

        /// <summary>
        /// The data container for chunk settings
        /// </summary>
        public ChunkConfiguration configuration;

        public MeshGenerator meshGenerator;

        private Dictionary<Vector3Int, Chunk> m_cache;
        private Vector3Int m_maxChunks;

        //-------------------------------------------------
        #region Monobehaviors

        protected void Awake()
        {
            if (world == null)
                world = GetComponent<World>();
        }

        protected void Start()
        {
            m_cache = new Dictionary<Vector3Int, Chunk>();
            m_maxChunks = new Vector3Int(
                world.size.x / configuration.size.x,
                world.size.y / configuration.size.y,
                world.size.z / configuration.size.z
            );
        }

        protected void OnDestroy()
        {
            meshGenerator.Dispose();
        }

        #endregion
        //-------------------------------------------------

        /// <summary>
        /// An enumeration of all managed chunks
        /// </summary>
        public IEnumerable<Chunk> allChunks {
            get { foreach (Chunk chunk in m_cache.Values) { yield return chunk; } }
        }

        /// <summary>
        /// Test if the Chunk index already exists in the cache
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public bool HasIndex(Vector3Int index) { return m_cache.ContainsKey(index); }

        /// <summary>
        /// Test if the Chunk index is valid
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public bool Contains(Vector3Int index)
        {
            if (index.x < 0 || index.x >= m_maxChunks.x) { return false; }
            if (index.y < 0 || index.y >= m_maxChunks.y) { return false; }
            if (index.z < 0 || index.z >= m_maxChunks.z) { return false; }
            return true;
        }

        /// <summary>
        /// Create a new Chunk in the world.
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public Chunk Create(Vector3Int index)
        {
            if (!Contains(index)) { return null; }

            Quaternion rotation = world.transform.rotation;
            Chunk chunk = UnityEngine.Object.Instantiate(
                chunkPrefab, Position(index), rotation) as Chunk;

            chunk.configuration = configuration;
            chunk.meshGenerator = meshGenerator;
            chunk.offset = new Vector3Int(
                index.x * configuration.size.x,
                index.y * configuration.size.y,
                index.z * configuration.size.z
            );
            chunk.Initialize();

            LinkChunkNeighbors(chunk, index);
            chunk.transform.parent = world.transform;
            m_cache.Add(index, chunk);
            return chunk;
        }

        /// <summary>
        /// Fetch a Chunk from the cache.
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public Chunk Get(Vector3Int index)
        {
            if (!Contains(index)) { return null; }
            if (!HasIndex(index)) { return null; }
            return m_cache[index];
        }

        /// <summary>
        /// Flags a Chunk as stale so it updates on the next frame
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public void Refresh(Vector3Int index)
        {
            if (!Contains(index)) { return; }
            if (!HasIndex(index)) { return; }
            m_cache[index].stale = true;
        }

        /// <summary>
        /// Remove a Chunk from the manager and destroy it
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public void Destroy(Vector3Int index)
        {
            if (!Contains(index)) { return; }
            if (!HasIndex(index)) { return; }

            UnLinkChunkNeighbors(index);
            UnityEngine.Object.Destroy(m_cache[index].gameObject);
            m_cache.Remove(index);
        }

        /// <summary>
        /// Calculate a Chunk's position in the scene, relative to the worlds transform
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public Vector3 Position(Vector3Int index)
        {
            Vector3 position = index;                           // adjust for the chunks offset
            position.x *= configuration.size.x;
            position.y *= configuration.size.y;
            position.z *= configuration.size.z;

            position.x += configuration.size.x * 0.5f;          // align the chunk with the world
            position.y += configuration.size.y * 0.5f;
            position.z += configuration.size.z * 0.5f;

            position += world.data.center * -1f;               // adjust for the worlds center
            position = world.transform.rotation * position;    // adjust for the worlds rotation
            position *= world.scale;                           // adjust for the worlds scale
            position += world.transform.position;              // adjust for the worlds position

            return position;
        }

        /// <summary>
        /// Calculates a Chunk index from a global position in the voxel grid
        /// </summary>
        /// <param name="point">A global position in the voxel grid</param>
        public Vector3Int IndexFrom(Vector3Int point)
        {
            Vector3Int index = Vector3Int.zero;
            index.x = point.x / configuration.size.x;
            index.y = point.y / configuration.size.y;
            index.z = point.z / configuration.size.z;
            return index;
        }

        /// <summary>
        /// Updates the Chunk containing the given grid point.
        /// Any adjacent Chunks will be updated if the point is on an edge.
        /// </summary>
        /// <param name="index">A point in the voxel grid</param>
        public void UpdateFrom(Vector3Int point)
        {
            Vector3Int index = IndexFrom(point);
            Refresh(index);

            // update neighboring chunks
            //
            // check if x is a local minimum for the chunk
            // and the chunk is not the first chunk on the X axis
            if (point.x - (index.x * configuration.size.x) == 0 && index.x != 0)
                Refresh(index + Direction3Int.West);

            // check if x is a local maximum for the chunk
            // and the chunk is not the last chunk on the X axis
            if (point.x - (index.x * configuration.size.x) == configuration.size.x - 1 && index.x != m_maxChunks.x - 1)
                Refresh(index + Direction3Int.East);

            // check if y is a local minimum for the chunk
            // and the chunk is not the first chunk on the Y axis
            if (point.y - (index.y * configuration.size.y) == 0 && index.y != 0)
                Refresh(index + Direction3Int.Down);

            // check if y is a local maximum for the chunk
            // and the chunk is not the last chunk on the Y axis
            if (point.y - (index.y * configuration.size.y) == configuration.size.y - 1 && index.y != m_maxChunks.y - 1)
                Refresh(index + Direction3Int.Up);

            // check if z is a local minimum for the chunk
            // and the chunk is not the first chunk on the Z axis
            if (point.z - (index.z * configuration.size.z) == 0 && index.z != 0)
                Refresh(index + Direction3Int.South);

            // check if z is a local maximum for the chunk
            // and the chunk is not the last chunk on the Z axis
            if (point.z - (index.z * configuration.size.z) == configuration.size.z - 1 && index.z != m_maxChunks.z - 1)
                Refresh(index + Direction3Int.North);
        }

        /// <summary>
        /// Create a batch of chunks in the world.
        /// Existing chunks will be marked as stale.
        /// Indexes that are out of bounds will be skipped.
        /// </summary>
        /// <param name="count">The number of Chunks to create in each dimension</param>
        /// <param name="offset">The offset for the Chunk indexes</param>
        public void Load(Vector3Int count, Vector3Int offset)
        {
            Vector3Int index = Vector3Int.zero;
            for (int x = 0; x < count.x; x++)
            {
                index.x = x + offset.x;
                for (int z = 0; z < count.z; z++)
                {
                    index.z = z + offset.z;
                    for (int y = 0; y < count.y; y++)
                    {
                        index.y = y + offset.y;
                        if (!Contains(index)) { continue; }
                        if (!HasIndex(index)) { Create(index); }
                        else { Refresh(index); }
                    }
                }
            }
        }

        /// <summary>
        /// Create all of the chunks in the world.
        /// Existing chunks will be marked as stale.
        /// </summary>
        public void LoadAll()
        {
            Vector3Int index = Vector3Int.zero;
            for (int x = 0; x < m_maxChunks.x; x++)
            {
                index.x = x;
                for (int z = 0; z < m_maxChunks.z; z++)
                {
                    index.z = z;
                    for (int y = 0; y < m_maxChunks.y; y++)
                    {
                        index.y = y;
                        if (!HasIndex(index)) { Create(index); }
                        else { Refresh(index); }
                    }
                }
            }
        }

        /// <summary>
        /// Link the chunk with the 6 adjacent neighbors
        /// </summary>
        protected void LinkChunkNeighbors(Chunk chunk, Vector3Int index)
        {
            Vector3Int next = Vector3Int.zero;
            Chunk nextChunk;

            for (int i = 0; i < Cube.Directions3Int.Length; i++)
            {
                next = index + Cube.Directions3Int[i];
                nextChunk = Get(next);

                if (nextChunk == null)
                    continue;

                switch (i)
                {
                    case 0: // Up
                        chunk.neighbors.up = nextChunk;
                        nextChunk.neighbors.down = chunk;
                        break;
                    case 1: // Down
                        chunk.neighbors.down = nextChunk;
                        nextChunk.neighbors.up = chunk;
                        break;
                    case 2: // North (Front)
                        chunk.neighbors.north = nextChunk;
                        nextChunk.neighbors.south = chunk;
                        break;
                    case 3: // East (Right)
                        chunk.neighbors.east = nextChunk;
                        nextChunk.neighbors.west = chunk;
                        break;
                    case 4: // South (Back)
                        chunk.neighbors.south = nextChunk;
                        nextChunk.neighbors.north = chunk;
                        break;
                    case 5: // West (Left)
                        chunk.neighbors.west = nextChunk;
                        nextChunk.neighbors.east = chunk;
                        break;
                }
            }
        }

        /// <summary>
        /// Unlink a chunk from the 6 adjacent neighbors
        /// </summary>
        protected void UnLinkChunkNeighbors(Vector3Int index)
        {
            Vector3Int next = Vector3Int.zero;
            Chunk nextChunk;

            for (int i = 0; i < Cube.Directions3Int.Length; i++)
            {
                next = index + Cube.Directions3Int[i];
                nextChunk = Get(next);

                if (nextChunk == null)
                    continue;

                switch (i)
                {
                    case 0: // Up
                        nextChunk.neighbors.down = null;
                        break;
                    case 1: // Down
                        nextChunk.neighbors.up = null;
                        break;
                    case 2: // North (Front)
                        nextChunk.neighbors.south = null;
                        break;
                    case 3: // East (Right)
                        nextChunk.neighbors.west = null;
                        break;
                    case 4: // South (Back)
                        nextChunk.neighbors.north = null;
                        break;
                    case 5: // West (Left)
                        nextChunk.neighbors.east = null;
                        break;
                }
            }
        }
    }
}
