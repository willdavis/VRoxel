using VRoxel.Core.Data;
using UnityEngine;

namespace VRoxel.Core
{
    public class World : MonoBehaviour
    {
        /// <summary>
        /// The chunk manager for this voxel world
        /// </summary>
        public ChunkManager chunks;

        /// <summary>
        /// The voxel dimensions for the world
        /// </summary>
        public Vector3Int size = Vector3Int.one;

        /// <summary>
        /// The size scale factor for the world
        /// </summary>
        [Range(0,100)]
        public float scale = 1f;

        /// <summary>
        /// The voxel data for the World
        /// </summary>
        public VoxelGrid data { get { return _data; } }
        private VoxelGrid _data;

        /// <summary>
        /// Initialize a new World
        /// </summary>
        public void Initialize()
        {
            _data = new VoxelGrid(size);
        }

        /// <summary>
        /// Tests if a scene position is inside the voxel world
        /// </summary>
        public bool Contains(Vector3 position)
        {
            Vector3Int point = WorldEditor.Get(this, position);
            return _data.Contains(point);
        }

        //-------------------------------------------------
        #region Monobehaviors

        protected void Awake()
        {
            if (chunks == null)
                chunks = GetComponent<ChunkManager>();
        }

        protected void OnDestroy()
        {
            if (_data != null)
                _data.Dispose();
        }

        protected void OnDrawGizmos()
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

        #endregion
        //-------------------------------------------------
    }
}
