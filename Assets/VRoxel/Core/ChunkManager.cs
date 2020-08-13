using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    public class ChunkManager
    {
        private World _world;
        private Chunk _prefab;
        private Vector3Int _max;
        private Dictionary<Vector3Int, Chunk> _cache;

        public MeshGenerator meshGenerator;

        /// <summary>
        /// Flag if all the chunks should have a collision mesh
        /// </summary>
        public bool collidable = true;

        public ChunkManager(World world, Chunk prefab)
        {
            _cache = new Dictionary<Vector3Int, Chunk>();

            _max = new Vector3Int(
                world.size.x / world.chunkSize.x,
                world.size.y / world.chunkSize.y,
                world.size.z / world.chunkSize.z
            );

            _prefab = prefab;
            _world = world;
        }

        /// <summary>
        /// An enumeration of all Chunks in the cache.
        /// </summary>
        public IEnumerable<Chunk> all {
            get { foreach (Chunk chunk in _cache.Values) { yield return chunk; } }
        }

        /// <summary>
        /// The maximum number of Chunks this World can have in each dimension.
        /// </summary>
        public Vector3Int max { get { return _max; } }

        /// <summary>
        /// Test if the Chunk index already exists in the cache
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public bool HasIndex(Vector3Int index) { return _cache.ContainsKey(index); }

        /// <summary>
        /// Test if the Chunk index is valid
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public bool Contains(Vector3Int index)
        {
            if (index.x < 0 || index.x >= _max.x) { return false; }
            if (index.y < 0 || index.y >= _max.y) { return false; }
            if (index.z < 0 || index.z >= _max.z) { return false; }
            return true;
        }

        /// <summary>
        /// Create a new Chunk in the world.
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public Chunk Create(Vector3Int index)
        {
            if (!Contains(index)) { return null; }

            Quaternion rotation = _world.transform.rotation;
            Chunk chunk = UnityEngine.Object.Instantiate(_prefab, Position(index), rotation) as Chunk;

            Data.ChunkConfiguration config = ScriptableObject
                .CreateInstance("ChunkConfiguration") as Data.ChunkConfiguration;

            config.scale = _world.scale;
            config.size = _world.chunkSize;
            config.collidable = collidable;
            config.material = _world.blocks.texture.material;

            chunk.configuration = config;
            chunk.meshGenerator = meshGenerator;
            chunk.offset = new Vector3Int(
                index.x * _world.chunkSize.x,
                index.y * _world.chunkSize.y,
                index.z * _world.chunkSize.z
            );

            chunk.transform.parent = _world.transform;
            _cache.Add(index, chunk);
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
            return _cache[index];
        }

        /// <summary>
        /// Flag a Chunk as stale so it updates on the next frame
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public void Update(Vector3Int index)
        {
            if (!Contains(index)) { return; }
            if (!HasIndex(index)) { return; }
            _cache[index].stale = true;
        }

        /// <summary>
        /// Remove a Chunk from the cache and destroy it
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public void Destroy(Vector3Int index)
        {
            if (!Contains(index)) { return; }
            if (!HasIndex(index)) { return; }

            Object.Destroy(_cache[index].gameObject);
            _cache.Remove(index);
        }

        /// <summary>
        /// Calculate a Chunk's position in the scene, relative to the worlds transform
        /// </summary>
        /// <param name="index">The Chunk index</param>
        public Vector3 Position(Vector3Int index)
        {
            Vector3 position = index;                           // adjust for the chunks offset
            position.x *= _world.chunkSize.x;
            position.y *= _world.chunkSize.y;
            position.z *= _world.chunkSize.z;

            position.x += _world.chunkSize.x * 0.5f;            // align the chunk with the world
            position.y += _world.chunkSize.y * 0.5f;
            position.z += _world.chunkSize.z * 0.5f;

            position += _world.data.center * -1f;               // adjust for the worlds center
            position = _world.transform.rotation * position;    // adjust for the worlds rotation
            position *= _world.scale;                           // adjust for the worlds scale
            position += _world.transform.position;              // adjust for the worlds position

            return position;
        }

        /// <summary>
        /// Calculates a Chunk index from a point in the voxel grid
        /// </summary>
        /// <param name="point">A point in the voxel grid</param>
        public Vector3Int IndexFrom(Vector3Int point)
        {
            Vector3Int index = Vector3Int.zero;
            index.x = point.x / _world.chunkSize.x;
            index.y = point.y / _world.chunkSize.y;
            index.z = point.z / _world.chunkSize.z;
            return index;
        }

        /// <summary>
        /// Updates the Chunk containing the given grid point.
        /// Any adjacent Chunks will be updated if the point is on an edge.
        /// </summary>
        /// <param name="index">A point in the voxel grid</param>
        public void UpdateFrom(Vector3Int point)
        {
            Vector3Int Vector3Int_front = new Vector3Int(0,0,1);
            Vector3Int Vector3Int_back = new Vector3Int(0,0,-1);
            Vector3Int index = IndexFrom(point);
            Update(index);

            // update neighboring chunks
            //
            // check if x is a local minimum for the chunk
            // and the chunk is not the first chunk on the X axis
            if (point.x - (index.x * _world.chunkSize.x) == 0 && index.x != 0)
                Update(index + Vector3Int.left);

            // check if x is a local maximum for the chunk
            // and the chunk is not the last chunk on the X axis
            if (point.x - (index.x * _world.chunkSize.x) == _world.chunkSize.x - 1 && index.x != _max.x - 1)
                Update(index + Vector3Int.right);

            // check if y is a local minimum for the chunk
            // and the chunk is not the first chunk on the Y axis
            if (point.y - (index.y * _world.chunkSize.y) == 0 && index.y != 0)
                Update(index + Vector3Int.down);

            // check if y is a local maximum for the chunk
            // and the chunk is not the last chunk on the Y axis
            if (point.y - (index.y * _world.chunkSize.y) == _world.chunkSize.y - 1 && index.y != _max.y - 1)
                Update(index + Vector3Int.up);

            // check if z is a local minimum for the chunk
            // and the chunk is not the first chunk on the Z axis
            if (point.z - (index.z * _world.chunkSize.z) == 0 && index.z != 0)
                Update(index + Vector3Int_back);

            // check if z is a local maximum for the chunk
            // and the chunk is not the last chunk on the Z axis
            if (point.z - (index.z * _world.chunkSize.z) == _world.chunkSize.z - 1 && index.z != _max.z - 1)
                Update(index + Vector3Int_front);
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
                        else { Update(index); }
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
            for (int x = 0; x < _max.x; x++)
            {
                index.x = x;
                for (int z = 0; z < _max.z; z++)
                {
                    index.z = z;
                    for (int y = 0; y < _max.y; y++)
                    {
                        index.y = y;
                        if (!HasIndex(index)) { Create(index); }
                        else { Update(index); }
                    }
                }
            }
        }
    }
}
