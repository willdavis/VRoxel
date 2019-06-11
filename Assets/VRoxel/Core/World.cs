using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    /// <summary>
    /// The voxel dimensions for the world
    /// </summary>
    public Vector3Int size = new Vector3Int(16, 16, 16);

    /// <summary>
    /// The number of voxels in a chunk
    /// </summary>
    public Vector3Int chunkSize = new Vector3Int(16, 16, 16);

    /// <summary>
    /// The number of chunks in a region
    /// </summary>
    public Vector3Int regionSize = new Vector3Int(1, 1, 1);

    /// <summary>
    /// The scale factor for the world
    /// </summary>
    public float scale = 1.0f;

    /// <summary>
    /// Unity prefab for a Chunk
    /// </summary>
    public Chunk chunk;

    /// <summary>
    /// Unity Material to use as the texture atlas
    /// </summary>
    public Material material;
}
