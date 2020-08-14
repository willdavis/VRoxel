using VRoxel.Core.Data;
using Unity.Collections;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// A partition of the voxel space
    /// </summary>
    [RequireComponent(
        typeof(MeshFilter),
        typeof(MeshRenderer),
        typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        /// <summary>
        /// The configuration settings for this chunk
        /// </summary>
        public ChunkConfiguration configuration;

        /// <summary>
        /// The generator used to create the voxel mesh
        /// </summary>
        public MeshGenerator meshGenerator;

        /// <summary>
        /// The chunks offset in the voxel space
        /// </summary>
        public Vector3Int offset;

        /// <summary>
        /// Flags if the Chunk needs to be updated
        /// </summary>
        [HideInInspector]
        public bool stale;


        protected Mesh m_mesh;
        protected MeshFilter m_meshFilter;
        protected MeshCollider m_meshCollider;
        protected MeshRenderer m_meshRenderer;

        /// <summary>
        /// The voxel blocks contained in this chunk
        /// </summary>
        protected NativeArray<byte> m_voxels;

        //-------------------------------------------------
        #region Read-Only Chunk Attributes

        /// <summary>
        /// Returns the scale factor for the chunks size
        /// </summary>
        public float scale { get { return configuration.scale; } }

        /// <summary>
        /// Returns the (x,y,z) dimensions of the chunk
        /// </summary>
        public Vector3Int size { get { return configuration.size; } }

        /// <summary>
        /// Returns the material used to texture the chunk
        /// </summary>
        public Material material { get { return configuration.material; } }

        /// <summary>
        /// Returns true if the chunk requires a collision mesh
        /// </summary>
        public bool collidable { get { return configuration.collidable; } }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Public API

        /// <summary>
        /// Initializes the Chunk's mesh and voxel data
        /// </summary>
        public void Initialize()
        {
            m_mesh = new Mesh();
            m_meshRenderer.material = material;
            m_voxels = new NativeArray<byte>(
                size.x * size.y * size.z, Allocator.Persistent
            );
        }

        /// <summary>
        /// Read the voxel at a position in the Chunk
        /// </summary>
        public byte Read(Vector3Int point)
        {
            if (!Contains(point)) { return byte.MaxValue; }
            return m_voxels[Flatten(point)];
        }

        /// <summary>
        /// Write voxel data at a position in the Chunk
        /// </summary>
        public void Write(Vector3Int point, byte block)
        {
            if (!Contains(point)) { return; }
            m_voxels[Flatten(point)] = block;
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshCollider = GetComponent<MeshCollider>();
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Start()
        {
            Initialize();
            GenerateMesh();
        }

        protected virtual void Update()
        {
            if (stale)
            {
                stale = false;
                GenerateMesh();
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_voxels != null)
                m_voxels.Dispose();
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(
                size.x * scale, size.y * scale, size.z * scale
            ));
        }

        #endregion
        //-------------------------------------------------


        /// <summary>
        /// Generates the render and collision mesh for the Chunk
        /// </summary>
        protected void GenerateMesh()
        {
            meshGenerator.BuildMesh(size, offset, ref m_mesh);
            m_meshFilter.sharedMesh = m_mesh;

            if (collidable) { m_meshCollider.sharedMesh = m_mesh; }
            else { m_meshCollider.sharedMesh = null; }
        }

        /// <summary>
        /// Test if a point is inside the voxel array
        /// </summary>
        protected bool Contains(Vector3Int point)
        {
            if (point.x < 0 || point.x >= size.x) { return false; }
            if (point.y < 0 || point.y >= size.y) { return false; }
            if (point.z < 0 || point.z >= size.z) { return false; }
            return true;
        }

        /// <summary>
        /// Calculate an array index from a Vector3Int point
        /// </summary>
        protected int Flatten(Vector3Int point)
        {
            /// A[x,y,z] = A[ x * height * depth + y * depth + z ]
            return (point.x * size.y * size.z) + (point.y * size.z) + point.z;
        }
    }
}
