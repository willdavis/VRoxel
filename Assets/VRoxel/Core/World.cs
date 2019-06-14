using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    /// <summary>
    /// Unity prefab for a Chunk
    /// </summary>
    public Chunk chunk;

    /// <summary>
    /// Unity Material to use as the texture atlas
    /// </summary>
    public Material material;

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
