using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    /// <summary>
    /// The size of the world in voxels
    /// </summary>
    public Vector3Int size = new Vector3Int(16, 16, 16);

    /// <summary>
    /// The scale factor for the world
    /// </summary>
    public float scale = 1.0f;

    /// <summary>
    /// The size of each chunk in voxels
    /// </summary>
    public Vector3Int chunkSize = new Vector3Int(16, 16, 16);

    /// <summary>
    /// Unity prefab for a Chunk
    /// </summary>
    public Chunk chunk;
}
