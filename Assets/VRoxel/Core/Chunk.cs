﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    [HideInInspector] public bool needsUpdate;

    private World _world;
    private Vector3Int _offset;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    /// <summary>
    /// Initialize a Chunk in the World
    /// </summary>
    public void Initialize(World world, Vector3Int offset)
    {
        _world = world;
        _offset = offset;
        _meshRenderer.material = world.blocks.textures.material;

        // TODO: configure mesh generator, for example:
        //_meshGenerator = new MeshGenerator(_world.data, _world.textures);
    }

    /// <summary>
    /// Generates the render and collision mesh for the Chunk
    /// </summary>
    private void GenerateMesh()
    {
        // TODO: run mesh generator, for example:
        //Mesh mesh = _meshGenerator.build(bounds, offset);
        //_meshFilter.sharedMesh = mesh;
        //_meshCollider.sharedMesh = mesh;
    }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
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
