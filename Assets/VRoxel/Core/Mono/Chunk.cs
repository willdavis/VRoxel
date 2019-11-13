﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        private Mesh _mesh;
        private World _world;
        private Vector3Int _offset;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private MeshGenerator _meshGenerator;

        /// <summary>
        /// Flags if the Chunk needs to be updated
        /// </summary>
        [HideInInspector]
        public bool stale;

        /// <summary>
        /// Flags if the Chunk needs a collision mesh
        /// </summary>
        [HideInInspector]
        public bool collidable = true;

        /// <summary>
        /// Initialize a Chunk in the World
        /// </summary>
        /// <param name="world">The parent World this Chunk belongs to</param>
        /// <param name="position">The chunks grid position in the world</param>
        public void Initialize(World world, Vector3Int position)
        {
            GetComponent<MeshRenderer>().material = world.blocks.texture.material;
            _meshGenerator = new MeshGenerator(world.data, world.blocks, world.scale);
            _mesh = new Mesh();
            _world = world;

            // set the chunks offset in the voxel grid
            _offset.x = position.x * world.chunkSize.x;
            _offset.y = position.y * world.chunkSize.y;
            _offset.z = position.z * world.chunkSize.z;
        }

        /// <summary>
        /// Generates the render and collision mesh for the Chunk
        /// </summary>
        public void GenerateMesh()
        {
            _meshGenerator.BuildMesh(_world.chunkSize, _offset, ref _mesh);
            _meshFilter.sharedMesh = _mesh;

            if (collidable) { _meshCollider.sharedMesh = _mesh; }
            else { _meshCollider.sharedMesh = null; }
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
            if (stale)
            {
                GenerateMesh();
                stale = false;
            }
        }

        void OnDrawGizmos()
        {
            Vector3 bounds = new Vector3(
                _world.chunkSize.x * _world.scale,
                _world.chunkSize.y * _world.scale,
                _world.chunkSize.z * _world.scale
            );

            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, bounds);
        }
    }
}