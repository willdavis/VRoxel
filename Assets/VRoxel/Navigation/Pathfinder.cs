using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    public struct Node
    {
        public Vector3Int index, parent;
        public float g, h;

        public Node(Vector3Int index, Vector3Int parent, float g = 0, float h = 0)
        {
            this.parent = parent;
            this.index = index;
            this.g = g;
            this.h = h;
        }
    }

    World _world;
    Vector3Int _start, _end;
    public Dictionary<Vector3Int, Node> _closed;
    List<Node> _neighbors;

    // TODO: Move these somewhere global and static
    Vector3Int _maxIndex = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
    Vector3Int Vector3Int_north = new Vector3Int(0,0,1);
    Vector3Int Vector3Int_south = new Vector3Int(0,0,-1);
    Vector3Int Vector3Int_north_east = new Vector3Int(1,0,1);
    Vector3Int Vector3Int_north_west = new Vector3Int(-1,0,1);
    Vector3Int Vector3Int_south_east = new Vector3Int(1,0,-1);
    Vector3Int Vector3Int_south_west = new Vector3Int(-1,0,-1);
    Vector3Int Vector3Int_top_north = new Vector3Int(0,1,1);
    Vector3Int Vector3Int_top_north_east = new Vector3Int(1,1,1);
    Vector3Int Vector3Int_top_north_west = new Vector3Int(-1,1,1);
    Vector3Int Vector3Int_top_south = new Vector3Int(0,1,-1);
    Vector3Int Vector3Int_top_south_east = new Vector3Int(1,1,-1);
    Vector3Int Vector3Int_top_south_west = new Vector3Int(-1,1,-1);
    Vector3Int Vector3Int_top_east = new Vector3Int(1,1,0);
    Vector3Int Vector3Int_top_west = new Vector3Int(-1,1,0);

    Vector3Int Vector3Int_bottom_north = new Vector3Int(0,-1,1);
    Vector3Int Vector3Int_bottom_north_east = new Vector3Int(1,-1,1);
    Vector3Int Vector3Int_bottom_north_west = new Vector3Int(-1,-1,1);

    Vector3Int Vector3Int_bottom_south = new Vector3Int(0,-1,-1);
    Vector3Int Vector3Int_bottom_south_east = new Vector3Int(1,-1,-1);
    Vector3Int Vector3Int_bottom_south_west = new Vector3Int(-1,-1,-1);

    Vector3Int Vector3Int_bottom_east = new Vector3Int(1,-1,0);
    Vector3Int Vector3Int_bottom_west = new Vector3Int(-1,-1,0);

    public Pathfinder(World world)
    {
        _world = world;
        _closed = new Dictionary<Vector3Int, Node>();
        _neighbors = new List<Node>();
    }

    /// <summary>
    /// Generates pathfinder nodes using a Breadth First Search
    /// </summary>
    public void BFS(Vector3Int start)
    {
        Queue<Node> frontier = new Queue<Node>();
        Node node = new Node(start, start, 0, 0);
        _closed.Clear();

        frontier.Enqueue(node);
        _closed.Add(start, node);
        //Debug.Log("Visited Node: " + node.index);
        
        while (frontier.Count != 0)
        {
            node = frontier.Dequeue();
            foreach (Node next in BFSNodeNeighbors(node))
            {
                if (next.index == _maxIndex) { continue; } // the node was not walkable
                if (_closed.ContainsKey(next.index)) { continue; } // the node has already been explored

                frontier.Enqueue(next);
                _closed.Add(next.index, next);
                //Debug.Log("Visited Node: " + next.index);
            }
        }

        Debug.Log("Total Node Size: " + _closed.Count);
    }

    private IEnumerable BFSNodeNeighbors(Node parent)
    {
        _neighbors.Clear();

        // Compass Directions
        // NORTH
        Vector3Int point = parent.index + Vector3Int_north;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // NORTH-EAST
        point = parent.index + Vector3Int_north_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // EAST
        point = parent.index + Vector3Int.right;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // SOUTH-EAST
        point = parent.index + Vector3Int_south_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // SOUTH
        point = parent.index + Vector3Int_south;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // SOUTH-WEST
        point = parent.index + Vector3Int_south_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // WEST
        point = parent.index + Vector3Int.left;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // NORTH-WEST
        point = parent.index + Vector3Int_north_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }


        // Top Compass Directions
        // TOP
        point = parent.index + Vector3Int.up;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-NORTH
        point = parent.index + Vector3Int_top_north;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-NORTH-EAST
        point = parent.index + Vector3Int_top_north_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-EAST
        point = parent.index + Vector3Int_top_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-SOUTH-EAST
        point = parent.index + Vector3Int_top_south_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-SOUTH
        point = parent.index + Vector3Int_top_south;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-SOUTH-WEST
        point = parent.index + Vector3Int_top_south_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-WEST
        point = parent.index + Vector3Int_top_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // TOP-NORTH-WEST
        point = parent.index + Vector3Int_top_north_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }


        // Down Directions
        // BOTTOM
        point = parent.index + Vector3Int.down;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-NORTH
        point = parent.index + Vector3Int_bottom_north;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-NORTH-EAST
        point = parent.index + Vector3Int_bottom_north_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-EAST
        point = parent.index + Vector3Int_bottom_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-SOUTH-EAST
        point = parent.index + Vector3Int_bottom_south_east;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-SOUTH
        point = parent.index + Vector3Int_bottom_south;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-SOUTH-WEST
        point = parent.index + Vector3Int_bottom_south_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-WEST
        point = parent.index + Vector3Int_bottom_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        // BOTTOM-NORTH-WEST
        point = parent.index + Vector3Int_bottom_north_west;
        if (IsWalkable(point)) { BFSSetNeighbor(point, parent.index); }

        return _neighbors;
    }

    private void BFSSetNeighbor(Vector3Int index, Vector3Int parent)
    {
        Node node = new Node(index, parent);
        _neighbors.Add(node);
    }

    /// <summary>
    /// Test if the grid point is a valid navigation node
    /// </summary>
    private bool IsWalkable(Vector3Int point)
    {
        return _world.data.Contains(point)                      // the point must be inside the world
            && _world.data.Get(point) == 0                      // the point must be an air block
            && _world.data.Get(point + Vector3Int.down) != 0;   // the point below must be solid
    }
}
