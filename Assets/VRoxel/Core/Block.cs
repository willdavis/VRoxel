﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace VRoxel.Core
{
    /// <summary>
    /// Blocks contain information about voxels in the world
    /// </summary>
    [Serializable]
    public class Block
    {
        public byte index;
        public string name;
        public Dictionary<Cube.Direction, Vector2> textures;

        public bool isSolid;
        public bool isStatic;

        public Block()
        {
            textures = new Dictionary<Cube.Direction, Vector2>();
        }
    }
}
