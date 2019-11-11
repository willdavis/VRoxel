using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

using VRoxel.Core;

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
    List<Node> _neighbors;
    Vector3Int _start, _end;

    SimplePriorityQueue<Node, float> _open;
    Dictionary<Vector3Int, Node> _closed;

    public Dictionary<Vector3Int, Node> nodes { get {  return _closed;} }

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
        _neighbors = new List<Node>();
        _closed = new Dictionary<Vector3Int, Node>();
        _open = new SimplePriorityQueue<Node, float>();
    }

    public void NextWaypoint(Vector3 point, ref Vector3 waypoint)
    {
        Vector3Int index = WorldEditor.Get(_world, point);
        if (!_closed.ContainsKey(index)) // no path exists
        {
            waypoint = point;
            return;
        }

        waypoint = WorldEditor.Get(_world, _closed[index].parent);
    }

    public void PathFrom(Vector3 point, ref List<Vector3> path)
    {
        Vector3Int index = WorldEditor.Get(_world, point);
        if (!_closed.ContainsKey(index)) { return; }    // no path exists

        Node node = _closed[index];
        while (node.index != _start)
        {
            path.Add(WorldEditor.Get(_world, node.parent));
            node = _closed[node.parent];
        }
    }

    /// <summary>
    /// Generates pathfinder nodes using a Breadth First Search
    /// </summary>
    public void BFS(Vector3Int start)
    {
        Queue<Node> frontier = new Queue<Node>();
        Node node = new Node(start, start, 0, 0);

        _start = start;
        _closed.Clear();

        frontier.Enqueue(node);
        _closed.Add(start, node);
        
        while (frontier.Count != 0)
        {
            node = frontier.Dequeue();
            foreach (Node next in NodeNeighbors(node))
            {
                if (_closed.ContainsKey(next.index)) { continue; } // the node has already been explored

                frontier.Enqueue(next);
                _closed.Add(next.index, next);
            }
        }
    }

    public void Dijkstra(Vector3Int start)
    {
        Node node = new Node(start, start, 0, 0);

        _start = start;
        _closed.Clear();
        _open.Clear();

        _open.Enqueue(node, 0f);
        _closed.Add(start, node);

        while (_open.Count != 0)
        {
            node = _open.Dequeue();
            foreach (Node next in NodeNeighbors(node))
            {
                if (!_closed.ContainsKey(next.index))
                {
                    _open.Enqueue(next, next.g);
                    _closed.Add(next.index, next);
                }
                else if (next.g < _closed[next.index].g)
                {
                    _open.Enqueue(next, next.g);
                    _closed[next.index] = next;
                }
            }
        }
    }

    private IEnumerable NodeNeighbors(Node parent)
    {
        _neighbors.Clear();

        // Compass Directions
        // NORTH
        Vector3Int point = parent.index + Vector3Int_north;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

        // NORTH-EAST
        point = parent.index + Vector3Int_north_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // EAST
        point = parent.index + Vector3Int.right;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

        // SOUTH-EAST
        point = parent.index + Vector3Int_south_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // SOUTH
        point = parent.index + Vector3Int_south;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

        // SOUTH-WEST
        point = parent.index + Vector3Int_south_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // WEST
        point = parent.index + Vector3Int.left;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

        // NORTH-WEST
        point = parent.index + Vector3Int_north_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }


        // Top Compass Directions
        // TOP
        point = parent.index + Vector3Int.up;
        if (IsClimbable(point)) { SetNeighbor(point, parent, 2f); }

        // TOP-NORTH
        point = parent.index + Vector3Int_top_north;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-NORTH-EAST
        point = parent.index + Vector3Int_top_north_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-EAST
        point = parent.index + Vector3Int_top_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-SOUTH-EAST
        point = parent.index + Vector3Int_top_south_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-SOUTH
        point = parent.index + Vector3Int_top_south;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-SOUTH-WEST
        point = parent.index + Vector3Int_top_south_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-WEST
        point = parent.index + Vector3Int_top_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // TOP-NORTH-WEST
        point = parent.index + Vector3Int_top_north_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }


        // Down Directions
        // BOTTOM
        point = parent.index + Vector3Int.down;
        if (IsClimbable(point)) { SetNeighbor(point, parent, 2f); }

        // BOTTOM-NORTH
        point = parent.index + Vector3Int_bottom_north;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        if (IsClimbable(point)) { SetNeighbor(point, parent, 4.414f); }

        // BOTTOM-NORTH-EAST
        point = parent.index + Vector3Int_bottom_north_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // BOTTOM-EAST
        point = parent.index + Vector3Int_bottom_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        if (IsClimbable(point)) { SetNeighbor(point, parent, 4.414f); }

        // BOTTOM-SOUTH-EAST
        point = parent.index + Vector3Int_bottom_south_east;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // BOTTOM-SOUTH
        point = parent.index + Vector3Int_bottom_south;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        if (IsClimbable(point)) { SetNeighbor(point, parent, 4.414f); }

        // BOTTOM-SOUTH-WEST
        point = parent.index + Vector3Int_bottom_south_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        // BOTTOM-WEST
        point = parent.index + Vector3Int_bottom_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        if (IsClimbable(point)) { SetNeighbor(point, parent, 4.414f); }

        // BOTTOM-NORTH-WEST
        point = parent.index + Vector3Int_bottom_north_west;
        if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
        //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

        return _neighbors;
    }

    private void SetNeighbor(Vector3Int index, Node parent, float cost)
    {
        _neighbors.Add(new Node(index, parent.index, parent.g + cost));
    }

    /// <summary>
    /// Test if the grid point is a valid navigation node
    /// </summary>
    private bool IsWalkable(Vector3Int point)
    {
        if (!_world.data.Contains(point)) { return false; }     // the point must be inside the world

        byte id = _world.data.Get(point.x, point.y, point.z);
        if (_world.blocks.IsSolid(id)) { return false; }        // the point must be an air block

        byte below = _world.data.Get(point + Vector3Int.down);
        if (!_world.blocks.IsSolid(below)) { return false; }    // the point below must be solid

        byte above = _world.data.Get(point + Vector3Int.up);
        if (_world.blocks.IsSolid(above)) { return false; }    // the point above must be air

        return true;
    }

    private bool IsClimbable(Vector3Int point)
    {
        if (!_world.data.Contains(point)) { return false; }     // the point must be inside the world

        byte id = _world.data.Get(point);
        if (_world.blocks.IsSolid(id)) { return false; }        // the point must be an air block

        byte below = _world.data.Get(point + Vector3Int.down);
        if (_world.blocks.IsSolid(below)) { return false; }     // the point below must be air

        byte above = _world.data.Get(point + Vector3Int.up);
        if (_world.blocks.IsSolid(above)) { return false; }     // the point above must be air

        // climbable if any adjacent N,S,E,W block is solid
        byte right = _world.data.Get(point + Vector3Int.right);
        if (_world.blocks.IsSolid(right)) { return true; }

        byte left = _world.data.Get(point + Vector3Int.left);
        if (_world.blocks.IsSolid(left)) { return true; }

        byte north = _world.data.Get(point + Vector3Int_north);
        if (_world.blocks.IsSolid(north)) { return true; }

        byte south = _world.data.Get(point + Vector3Int_south);
        if (_world.blocks.IsSolid(south)) { return true; }

        // the point was not climbable
        return false;
    }
}
