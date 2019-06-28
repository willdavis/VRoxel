using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    [HideInInspector]
    public bool needsUpdate;

    private Mesh _mesh;
    private World _world;
    private Vector3Int _offset;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private MeshGenerator _meshGenerator;

    /// <summary>
    /// Initialize a Chunk in the World
    /// </summary>
    public void Initialize(World world, Vector3Int offset)
    {
        GetComponent<MeshRenderer>().material = world.blocks.textures.material;
        _meshGenerator = new MeshGenerator(world.data, world.blocks);
        _mesh = new Mesh();
        _offset = offset;
        _world = world;
    }

    /// <summary>
    /// Generates the render and collision mesh for the Chunk
    /// </summary>
    public void GenerateMesh()
    {
        _meshGenerator.BuildMesh(_world.chunkSize, _offset, _world.scale, ref _mesh);
        _meshFilter.sharedMesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
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

    void Update()
    {
        if (needsUpdate)
        {
            GenerateMesh();
            needsUpdate = false;
        }
    }
}
