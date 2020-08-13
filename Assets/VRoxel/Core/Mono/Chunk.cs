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
        protected virtual void GenerateMesh()
        {
            meshGenerator.BuildMesh(size, offset, ref m_mesh);
            m_meshFilter.sharedMesh = m_mesh;

            if (collidable) { m_meshCollider.sharedMesh = m_mesh; }
            else { m_meshCollider.sharedMesh = null; }
        }
    }
}
