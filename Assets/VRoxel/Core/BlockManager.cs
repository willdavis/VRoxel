using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages and configures all blocks in a world
/// </summary>
public class BlockManager
{
    /// <summary>
    /// The texture data common to all blocks
    /// </summary>
    public class TextureAtlas
    {
        /// <summary>
        /// The spacing between block textures in the atlas
        /// </summary>
        public float size;

        /// <summary>
        /// The source material for the texture atlas
        /// </summary>
        public Material material;
    }

    /// <summary>
    /// The texture data common to all blocks
    /// </summary>
    public TextureAtlas textures;
    
    /// <summary>
    /// Connects each block to a byte index used in the VoxelGrid
    /// </summary>
    public Dictionary<byte, Block> blocks;

    public BlockManager()
    {
        blocks = new Dictionary<byte, Block>();
        textures = new TextureAtlas();
    }
}
