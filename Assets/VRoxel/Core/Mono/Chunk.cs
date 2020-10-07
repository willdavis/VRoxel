using VRoxel.Core.Chunks;
using VRoxel.Core.Data;
using Unity.Collections;
using Unity.Jobs;
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
        /// The chunks offset in the voxel space
        /// </summary>
        public Vector3Int offset;

        /// <summary>
        /// The configuration settings for this chunk
        /// </summary>
        public ChunkConfiguration configuration;

        /// <summary>
        /// The generator used to create the voxel mesh
        /// </summary>
        [HideInInspector] public MeshGenerator meshGenerator;

        /// <summary>
        /// A reference to the 6 adjacent chunks
        /// </summary>
        [HideInInspector] public ChunkNeighbors neighbors;

        /// <summary>
        /// Flags if the Chunk needs to be updated
        /// </summary>
        [HideInInspector] public bool stale;

        protected Mesh m_mesh;
        protected MeshFilter m_meshFilter;
        protected MeshCollider m_meshCollider;
        protected MeshRenderer m_meshRenderer;

        /// <summary>
        /// The voxel data contained in this chunk
        /// </summary>
        public NativeArray<byte> voxels { get { return m_voxels; } }
        protected NativeArray<byte> m_voxels;

        public JobHandle buildingMesh { get; private set; }
        protected BuildChunkMesh m_buildChunkMesh;
        protected bool m_buildingMesh;

        protected NativeList<Vector3> m_vertices;
        protected NativeList<int> m_triangles;
        protected NativeList<Vector2> m_uvs;

        //-------------------------------------------------
        #region Read-Only Chunk Attributes

        /// <summary>
        /// Returns the scale factor for the chunks size
        /// </summary>
        public float scale { get { return configuration.sizeScale; } }

        /// <summary>
        /// Returns the (x,y,z) dimensions of the chunk
        /// </summary>
        public Vector3Int size { get { return configuration.size; } }

        /// <summary>
        /// Returns the texture atlas for the chunk
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
            int size1D = size.x * size.y * size.z;
            m_voxels = new NativeArray<byte>(size1D, Allocator.Persistent);

            m_vertices = new NativeList<Vector3>(Allocator.Persistent);
            m_triangles = new NativeList<int>(Allocator.Persistent);
            m_uvs = new NativeList<Vector2>(Allocator.Persistent);
            m_buildChunkMesh = new BuildChunkMesh();
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

        /// <summary>
        /// Dispose all unmanaged memory from the Chunk
        /// </summary>
        public void Dispose()
        {
            if (m_voxels.IsCreated)
                m_voxels.Dispose();

            if (m_vertices.IsCreated)
                m_vertices.Dispose();

            if (m_triangles.IsCreated)
                m_triangles.Dispose();

            if (m_uvs.IsCreated)
                m_uvs.Dispose();
        }

        #endregion
        //-------------------------------------------------


        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            m_mesh = new Mesh();
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshCollider = GetComponent<MeshCollider>();
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        protected virtual void Start()
        {
            m_meshRenderer.material = material;

            Initialize();
            stale = true;
        }

        protected virtual void Update()
        {
            if (stale) { GenerateMesh(); }
            if (m_buildingMesh) { UpdateMesh(); }
        }

        protected virtual void OnDestroy()
        {
            Dispose();
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
            stale = false;
            m_buildingMesh = true;
            buildingMesh = meshGenerator.BuildMesh(
                this, ref m_buildChunkMesh, ref m_vertices,
                ref m_triangles, ref m_uvs);
        }

        protected void UpdateMesh()
        {
            buildingMesh.Complete();
            m_buildingMesh = false;

            /// new rendering method
            //m_mesh.vertices = m_vertices.ToArray();
            //m_mesh.triangles = m_triangles.ToArray();
            //m_mesh.uv = m_uvs.ToArray();
            //m_mesh.RecalculateNormals();

            /// old rendering method
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
