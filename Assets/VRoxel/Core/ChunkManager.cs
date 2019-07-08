using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    private World _world;
    private Chunk _prefab;
    private Vector3Int _max;
    private Dictionary<Vector3Int, Chunk> _cache;

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
    /// Create a new Chunk in the world
    /// </summary>
    /// <param name="index">The Chunk index</param>
    public Chunk Create(Vector3Int index)
    {
        Quaternion rotation = _world.transform.rotation;
        Chunk chunk = UnityEngine.Object.Instantiate(_prefab, Position(index), rotation) as Chunk;

        chunk.transform.parent = _world.transform;
        chunk.Initialize(_world, index);
        _cache.Add(index, chunk);
        return chunk;
    }

    /// <summary>
    /// Fetch a Chunk from the cache
    /// </summary>
    /// <param name="index">The Chunk index</param>
    public Chunk Get(Vector3Int index)
    {
        if (!Contains(index)) { return null; }
        if (!_cache.ContainsKey(index)) { return null; }
        return _cache[index];
    }

    /// <summary>
    /// Flag a Chunk as stale so it updates on the next frame
    /// </summary>
    /// <param name="index">The Chunk index</param>
    public void Update(Vector3Int index)
    {
        if (!Contains(index)) { return; }
        if (!_cache.ContainsKey(index)) { return; }
        _cache[index].stale = true;
    }

    /// <summary>
    /// Remove a Chunk from the cache and destroy it
    /// </summary>
    /// <param name="index">The Chunk index</param>
    public void Destroy(Vector3Int index)
    {
        if (!Contains(index)) { return; }
        if (!_cache.ContainsKey(index)) { return; }

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
}
