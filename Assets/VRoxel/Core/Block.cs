using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Blocks contain information about voxels in the world
/// </summary>
public class Block
{
    public byte index;
    public string name;
    public Dictionary<Cube.Direction, Vector2> textures;

    public bool isSolid;

    public Block()
    {
        textures = new Dictionary<Cube.Direction, Vector2>();
    }
}
