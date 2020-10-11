using VRoxel.Core.Data;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// Defines a voxel space in the scene
    /// </summary>
    public class World : MonoBehaviour
    {
        /// <summary>
        /// A reference to the block manager for this world
        /// </summary>
        public BlockManager blockManager;

        /// <summary>
        /// A reference to the chunk manager for this world
        /// </summary>
        public ChunkManager chunkManager;

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
        /// Flags if the outer faces of the world should be rendered
        /// </summary>
        public bool renderWorldEdges = true;

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
        /// Checks if a scene position is inside the voxel world
        /// </summary>
        public bool Contains(Vector3 position)
        {
            return Contains(SceneToGrid(position));
        }

        /// <summary>
        /// Checks if a grid position is inside the voxel world
        /// </summary>
        public bool Contains(Vector3Int position)
        {
            if (position.x < 0 || position.x >= size.x) { return false; }
            if (position.y < 0 || position.y >= size.y) { return false; }
            if (position.z < 0 || position.z >= size.z) { return false; }
            return true;
        }

        /// <summary>
        /// Adjusts a RaycastHit point to be inside or outside the block that was hit
        /// </summary>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="direction">choose inside or outside the cube</param>
        public Vector3 AdjustRaycastHit(RaycastHit hit, Cube.Point direction)
        {
            Vector3 position = hit.point;
            switch (direction)
            {
                case Cube.Point.Inside:
                    position += hit.normal * (scale / -2f);
                    break;
                case Cube.Point.Outside:
                    position += hit.normal * (scale / 2f);
                    break;
            }
            return position;
        }

        /// <summary>
        /// Calculates a grid position in the voxel world from a position in the scene
        /// </summary>
        /// <param name="position">A position in the scene</param>
        public Vector3Int SceneToGrid(Vector3 position)
        {
            Vector3 adjusted = position;
            Vector3Int point = Vector3Int.zero;
            Quaternion rotation = Quaternion.Inverse(transform.rotation);

            adjusted += transform.position * -1f;         // adjust for the worlds position
            adjusted *= 1 / scale;                        // adjust for the worlds scale
            adjusted = rotation * adjusted;               // adjust for the worlds rotation
            adjusted += data.center;                      // adjust for the worlds center

            point.x = Mathf.FloorToInt(adjusted.x);
            point.y = Mathf.FloorToInt(adjusted.y);
            point.z = Mathf.FloorToInt(adjusted.z);

            return point;
        }

        /// <summary>
        /// Calculates a scene position from a grid point in the voxel world
        /// </summary>
        /// <param name="point">A grid position in the voxel world</param>
        public Vector3 GridToScene(Vector3Int point)
        {
            Vector3 position = point;
            position += Vector3.one * 0.5f;              // adjust for the chunks center
            position += data.center * -1f;               // adjust for the worlds center
            position = transform.rotation * position;    // adjust for the worlds rotation
            position *= scale;                           // adjust for the worlds scale
            position += transform.position;              // adjust for the worlds position
            return position;
        }

        /// <summary>
        /// Returns the voxel at a grid position in the world
        /// </summary>
        /// <param name="point">A grid position in the voxel world</param>
        public byte Read(Vector3Int point)
        {
            if (!Contains(point)) { return byte.MaxValue; }

            Vector3Int chunkIndex = chunkManager.IndexFrom(point);
            Chunk chunk = chunkManager.Get(chunkIndex);
            if (chunk == null) { return byte.MaxValue; }

            Vector3Int localPos = point - chunk.offset;
            return chunk.Read(localPos);
        }

        /// <summary>
        /// Updates the voxel at a grid position in the world
        /// </summary>
        /// <param name="point">A grid position in the voxel world</param>
        /// <param name="voxel">The block index that will be added</param>
        public void Write(Vector3Int point, byte voxel)
        {
            if (!Contains(point)) { return; }   // point is out of bounds

            Vector3Int chunkIndex = chunkManager.IndexFrom(point);
            Chunk chunk = chunkManager.Get(chunkIndex);
            if (chunk == null) { return; }      // the chunk is missing

            Vector3Int localPos = point - chunk.offset;
            byte original = chunk.Read(localPos);
            if (voxel == original) { return; }  // skip duplicate voxels

            BlockConfiguration originalBlock = blockManager.blocks[original];
            if (!originalBlock.editable) { return; } // block can not be changed

            chunk.Write(localPos, voxel);
            chunkManager.UpdateFrom(point);

            // deprecated but still needed for navigation
            data.Set(point.x, point.y, point.z, voxel);
        }

        //-------------------------------------------------
        #region Monobehaviors

        protected void Awake()
        {
            if (blockManager == null)
                blockManager = GetComponent<BlockManager>();
            if (chunkManager == null)
                chunkManager = GetComponent<ChunkManager>();
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
