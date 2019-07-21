using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private Terrain _terrain;
    private VoxelGrid _data;
    private ChunkManager _chunks;
    private AgentManager _agents;


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
    /// The NPC Agents in the world
    /// </summary>
    public AgentManager agents { get { return _agents; } }

    /// <summary>
    /// The random terrain generator
    /// </summary>
    public Terrain terrain { get { return _terrain; } }

    /// <summary>
    /// Initialize a new World
    /// </summary>
    public void Initialize()
    {
        _agents = new AgentManager(this);
        _chunks = new ChunkManager(this, chunk);
        _terrain = new Terrain(seed, 0.25f, 25f, 10f);
        _data = new VoxelGrid(size);
    }

    public bool Contains(Vector3 position)
    {
        Vector3Int point = WorldEditor.Get(this, position);
        return _data.Contains(point);
    }

    /// <summary>
    /// Generate world data within the given bounds.
    /// Any points outside the world will be skipped.
    /// </summary>
    /// <param name="size">The number of voxels to generate</param>
    /// <param name="offset">The offset from the world origin</param>
    public void Generate(Vector3Int size, Vector3Int offset)
    {
        int terrain;
        Vector3Int point = Vector3Int.zero;
        for (int x = 0; x < size.x; x++)
        {
            point.x = x + offset.x;
            for (int z = 0; z < size.z; z++)
            {
                point.z = z + offset.z;
                terrain = _terrain.GetHeight(point.x, point.z);
                for (int y = 0; y < size.y; y++)
                {                    
                    point.y = y + offset.y;
                    if (!_data.Contains(point)) { continue; }
                    if (point.y == 0) { _data.Set(point, 1); }
                    if (point.y <= terrain) { _data.Set(point, 1); }
                }
            }
        }
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
