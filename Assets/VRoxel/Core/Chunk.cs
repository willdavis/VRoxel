using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    /// <summary>
    /// Initialize a new Chunk in a World
    /// </summary>
    public void Initialize(World world, Vector3Int position)
    {

    }

    /// <summary>
    /// Generates the render and collision mesh for the Chunk
    /// </summary>
    private void GenerateMesh()
    {
        // TODO: something like this
        //Mesh mesh = _meshGenerator.build(data);
        //_meshFilter.sharedMesh = mesh;
        //_meshCollider.sharedMesh = mesh;
    }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    void Start()
    {
        GenerateMesh();
    }
}
