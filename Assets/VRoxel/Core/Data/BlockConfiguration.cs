using UnityEngine;

namespace VRoxel.Core.Data
{
    /// <summary>
    /// Data container for common block settings
    /// </summary>
    [CreateAssetMenu(fileName = "BlockConfiguration.asset", menuName = "VRoxel/Block Configuration", order = 1)]
    public class BlockConfiguration : ScriptableObject
    {
        /// <summary>
        /// The name of this block for display in the UI
        /// </summary>
        public new string name;

        /// <summary>
        /// A short description of this block for display in the UI
        /// </summary>
        [Multiline] public string description;


        /// <summary>
        /// Determines if this block needs a collision mesh
        /// </summary>
        [Header("Block Properties")]
        public bool collidable;

        /// <summary>
        /// Determines if this block can be edited by the player
        /// </summary>
        public bool editable;


        [Header("Texture Coordinates")]
        public Vector2 textureUp;
        public Vector2 textureDown;
        public Vector2 textureFront;
        public Vector2 textureBack;
        public Vector2 textureLeft;
        public Vector2 textureRight;
    }
}