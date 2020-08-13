using Unity.Collections;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// A partition of the voxel space
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
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
        /// The material used by the mesh generator to texture the chunk
        /// </summary>
        public Material material;

        /// <summary>
        /// The generator used to create the voxel mesh
        /// </summary>
        public MeshGenerator meshGenerator;

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


        protected Mesh m_mesh;
        protected MeshFilter m_meshFilter;
        protected MeshCollider m_meshCollider;
        protected MeshRenderer m_meshRenderer;

        /// <summary>
        /// The voxel blocks contained in this chunk
        /// </summary>
        protected NativeArray<byte> m_voxels;


        //-------------------------------------------------
        #region Public API

        /// <summary>
        /// Read the voxel at a position in the Chunk
        /// </summary>
        public byte Read(Vector3Int point)
        {
            if (!Contains(point)) { return 0; }
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
            int flatSize = size.x * size.y * size.z;

            m_mesh = new Mesh();
            m_meshRenderer.material = material;
            m_voxels = new NativeArray<byte>(flatSize, Allocator.Persistent);

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
