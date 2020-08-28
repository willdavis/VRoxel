using VRoxel.Core.Data;
using UnityEngine;

namespace VRoxel.Core
{
    public class World : MonoBehaviour
    {
        private VoxelGrid _data;
        private ChunkManager _chunks;


        /// <summary>
        /// Unity prefab for a Chunk
        /// </summary>
        public Chunk chunk;

        /// <summary>
        /// The scale factor for the world
        /// </summary>
        public float scale = 1f;

        /// <summary>
        /// The voxel dimensions for the world
        /// </summary>
        public Vector3Int size = Vector3Int.one;

        /// <summary>
        /// The number of voxels in a chunk
        /// </summary>
        public Vector3Int chunkSize = Vector3Int.one;

        /// <summary>
        /// The voxel data for the World
        /// </summary>
        public VoxelGrid data { get { return _data; } }

        /// <summary>
        /// The Chunk data for the world
        /// </summary>
        public ChunkManager chunks { get { return _chunks; } }

        /// <summary>
        /// Initialize a new World
        /// </summary>
        public void Initialize()
        {
            _data = new VoxelGrid(size);
            _chunks = new ChunkManager(this, chunk);
        }

        void OnDestroy()
        {
            if (_data != null)
                _data.Dispose();
        }

        public bool Contains(Vector3 position)
        {
            Vector3Int point = WorldEditor.Get(this, position);
            return _data.Contains(point);
        }

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
}
