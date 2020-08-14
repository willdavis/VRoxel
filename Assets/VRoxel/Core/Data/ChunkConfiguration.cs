using UnityEngine;

namespace VRoxel.Core.Data
{
    /// <summary>
    /// Data container for chunk settings
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkConfiguration.asset", menuName = "VRoxel/Chunk Configuration", order = 1)]
    public class ChunkConfiguration : ScriptableObject
    {
        /// <summary>
        /// The scale factor for the chunks size
        /// </summary>
        public float scale = 1f;

        /// <summary>
        /// The (x,y,z) dimensions of the chunk
        /// </summary>
        public Vector3Int size = Vector3Int.one;

        /// <summary>
        /// The material used by the mesh generator to texture the chunk
        /// </summary>
        public Material material;

        /// <summary>
        /// Flags if the Chunk needs a collision mesh
        /// </summary>
        public bool collidable = true;
    }
}