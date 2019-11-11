using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private VoxelGrid _data;
    private ChunkManager _chunks;


    /// <summary>
    /// Unity prefab for a Chunk
    /// </summary>
    public Chunk chunk;

    /// <summary>
    /// Contains the block definitions for this world
    /// </summary>
    public BlockManager blocks = new BlockManager();

    /// <summary>
    /// The seed for random noise generation
    /// </summary>
    public int seed = 0;

    /// <summary>
    /// The scale factor for the world
    /// </summary>
    public float scale = 1f;

    /// <summary>
    /// The voxel dimensions for the world
    /// </summary>
    public Vector3Int size = Vector3Int.one;

    /// <summary>
    /// The number of voxels in a chunk
    /// </summary>
    public Vector3Int chunkSize = Vector3Int.one;

    /// <summary>
    /// The number of chunks in a region
    /// </summary>
    public Vector3Int regionSize = Vector3Int.one;

    /// <summary>
    /// The voxel data for the World
    /// </summary>
    public VoxelGrid data { get { return _data; } }

    /// <summary>
    /// The Chunk data for the world
    /// </summary>
    public ChunkManager chunks { get { return _chunks; } }

    /// <summary>
    /// Initialize a new World
    /// </summary>
    public void Initialize()
    {
        _chunks = new ChunkManager(this, chunk);
        _data = new VoxelGrid(size);
    }

    public bool Contains(Vector3 position)
    {
        Vector3Int point = WorldEditor.Get(this, position);
        return _data.Contains(point);
    }

    public Block GetBlock(Vector3Int index)
    {
        if (!_data.Contains(index)) { return null; }

        byte id = _data.Get(index.x, index.y, index.z);
        if (!blocks.library.ContainsKey(id)) { return null; }

        return blocks.library[id];
    }

    void OnDrawGizmos()
    {
        Vector3 bounds = new Vector3(
            size.x * scale,
            size.y * scale,
            size.z * scale
        );

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, bounds);
    }
}
