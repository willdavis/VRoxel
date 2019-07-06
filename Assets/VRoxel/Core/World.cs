using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private Terrain _terrain;
    private VoxelGrid _data;


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
    /// The active Chunks in the World
    /// </summary>
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    /// <summary>
    /// Initialize a new World
    /// </summary>
    public void Initialize()
    {
        _terrain = new Terrain(seed);
        _data = new VoxelGrid(size);
    }

    /// <summary>
    /// Generate world data within the given bounds.
    /// Any points outside the world will be skipped.
    /// </summary>
    /// <param name="bounds">The area to generate data</param>
    /// <param name="offset">The offset from the world origin</param>
    public void Generate(Vector3Int bounds, Vector3Int offset)
    {
        Vector3Int point = Vector3Int.zero;
        for (int x = 0; x < bounds.x; x++)
        {
            for (int z = 0; z < bounds.z; z++)
            {
                for (int y = 0; y < bounds.y; y++)
                {
                    point.x = x + offset.x;
                    point.y = y + offset.y;
                    point.z = z + offset.z;

                    if (!_data.Contains(point)) { continue; }
                    _data.Set(point, 1); // default to (byte)1
                }
            }
        }
    }

    /// <summary>
    /// Creates a new Chunk in the World
    /// </summary>
    /// <param name="offset">The chunk offset from the world origin</param>
    public Chunk CreateChunk(Vector3Int offset)
    {
        Vector3 position = GetChunkPosition(offset);
        Quaternion rotation = transform.rotation;

        Chunk newChunk = Instantiate(chunk, position, rotation) as Chunk;
        newChunk.transform.parent = transform;
        newChunk.Initialize(this, offset);

        chunks.Add(offset, newChunk);
        return newChunk;
    }

    /// <summary>
    /// Returns the position for a Chunk in the Unity scene
    /// </summary>
    /// <param name="offset">The chunk offset from the world origin</param>
    public Vector3 GetChunkPosition(Vector3Int offset)
    {
        Vector3 position = offset;                  // adjust for the chunks offset
        position.x *= chunkSize.x;
        position.y *= chunkSize.y;
        position.z *= chunkSize.z;

        position.x += chunkSize.x * 0.5f;           // align the chunk with the world
        position.y += chunkSize.y * 0.5f;
        position.z += chunkSize.z * 0.5f;

        position += _data.center * -1f;             // adjust for the worlds center
        position = transform.rotation * position;   // adjust for the worlds rotation
        position *= scale;                          // adjust for the worlds scale
        position += transform.position;             // adjust for the worlds position

        return position;
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
