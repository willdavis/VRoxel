using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        private Mesh _mesh;
        private World _world;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private MeshRenderer _meshRenderer;

        /// <summary>
        /// The scale factor for the chunks size
        /// </summary>
        public float scale;

        /// <summary>
        /// The (x,y,z) dimensions of the chunk
        /// </summary>
        public Vector3Int size;

        /// <summary>
        /// The chunks offset in the voxel space
        /// </summary>
        public Vector3Int offset;

        /// <summary>
        /// The generator used to create the voxel mesh
        /// </summary>
        public MeshGenerator meshGenerator;

        /// <summary>
        /// The material used by the mesh generator to texture the chunk
        /// </summary>
        public Material material;

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
            _mesh = new Mesh();
            _world = world;
        }

        /// <summary>
        /// Generates the render and collision mesh for the Chunk
        /// </summary>
        public void GenerateMesh()
        {
            meshGenerator.BuildMesh(size, offset, ref _mesh);
            _meshFilter.sharedMesh = _mesh;

            if (collidable) { _meshCollider.sharedMesh = _mesh; }
            else { _meshCollider.sharedMesh = null; }
        }

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        void Start()
        {
            _meshRenderer.material = material;
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
                size.x * scale,
                size.y * scale,
                size.z * scale
            );

            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, bounds);
        }
    }
}
