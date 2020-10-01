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
        /// The (x,y,z) dimensions of the chunk
        /// </summary>
        public Vector3Int size = Vector3Int.one;

        /// <summary>
        /// The scale factor for the chunks size
        /// </summary>
        [Range(0, 10)]
        public float sizeScale = 1f;

        /// <summary>
        /// The material atlas used by the mesh generator to texture the chunk
        /// </summary>
        public Material material;

        /// <summary>
        /// The spacing between textures in the atlas material
        /// </summary>
        [Range(0,1)]
        public float textureScale = 1f;

        /// <summary>
        /// Flags if the Chunk needs a collision mesh
        /// </summary>
        public bool collidable = true;
    }
}