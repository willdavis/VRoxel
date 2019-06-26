using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private Terrain _terrain;
    private WorldData _data;


    /// <summary>
    /// Unity prefab for a Chunk
    /// </summary>
    public Chunk chunk;

    /// <summary>
    /// Unity Material to use as the texture atlas
    /// </summary>
    public Material material;

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
    public Vector3Int size = new Vector3Int(256, 256, 256);

    /// <summary>
    /// The number of voxels in a chunk
    /// </summary>
    public Vector3Int chunkSize = new Vector3Int(32, 32, 32);

    /// <summary>
    /// The number of chunks in a region
    /// </summary>
    public Vector3Int regionSize = new Vector3Int(8, 8, 8);

    /// <summary>
    /// The voxel data for the World
    /// </summary>
    public WorldData data { get { return _data; } }

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
        _data = new WorldData(size);
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
    /// Create a new Chunk in the World
    /// </summary>
    /// <param name="offset">The chunk offset from the world origin</param>
    public Chunk CreateChunk(Vector3Int offset)
    {
        Chunk newChunk = Instantiate(chunk, Vector3.zero, Quaternion.identity) as Chunk;
        chunks.Add(offset, newChunk);
        return newChunk;
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
