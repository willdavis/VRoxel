using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Blocks contain information about voxels in the world
/// </summary>
public class Block
{
    public string name;
    public Dictionary<Cube.Direction, Vector2> textures;
}
