using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    public struct Node
    {
        public Vector3Int index;
        public Vector3Int parent;
        public float g, h;
        public float f { get { return g + h; } }

        public Node(Vector3Int index, Vector3Int parent, float g, float h)
        {
            this.parent = parent;
            this.index = index;
            this.g = g;
            this.h = h;
        }
    }

    World _world;

    public Pathfinder(World world)
    {
        _world = world;
    }
}
