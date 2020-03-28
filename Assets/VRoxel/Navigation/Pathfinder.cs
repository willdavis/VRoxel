using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

using VRoxel.Core;

namespace VRoxel.Navigation
{
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

        public Dictionary<Vector3Int, Node> nodes { get {  return _closed;} }

        World _world;
        List<Node> _neighbors;
        Vector3Int _start, _end;

        SimplePriorityQueue<Node, float> _open;
        Dictionary<Vector3Int, Node> _closed;
        Vector3Int _maxIndex = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

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
            Vector3Int point = parent.index + Direction3Int.North;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // NORTH-EAST
            point = parent.index + Direction3Int.NorthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // EAST
            point = parent.index + Direction3Int.East;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // SOUTH-EAST
            point = parent.index + Direction3Int.SouthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // SOUTH
            point = parent.index + Direction3Int.South;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // SOUTH-WEST
            point = parent.index + Direction3Int.SouthWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // WEST
            point = parent.index + Direction3Int.West;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // NORTH-WEST
            point = parent.index + Direction3Int.NorthWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }


            // Top Compass Directions
            // TOP
            point = parent.index + Direction3Int.Up;
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // TOP-NORTH
            point = parent.index + Direction3Int.UpNorth;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-NORTH-EAST
            point = parent.index + Direction3Int.UpNorthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-EAST
            point = parent.index + Direction3Int.UpEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-SOUTH-EAST
            point = parent.index + Direction3Int.UpSouthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-SOUTH
            point = parent.index + Direction3Int.UpSouth;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-SOUTH-WEST
            point = parent.index + Direction3Int.UpSouthWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-WEST
            point = parent.index + Direction3Int.UpWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // TOP-NORTH-WEST
            point = parent.index + Direction3Int.UpNorthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }


            // Down Directions
            // BOTTOM
            point = parent.index + Direction3Int.Down;
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1f); }

            // BOTTOM-NORTH
            point = parent.index + Direction3Int.DownNorth;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-NORTH-EAST
            point = parent.index + Direction3Int.DownNorthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-EAST
            point = parent.index + Direction3Int.DownEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-SOUTH-EAST
            point = parent.index + Direction3Int.DownSouthEast;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-SOUTH
            point = parent.index + Direction3Int.DownSouth;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-SOUTH-WEST
            point = parent.index + Direction3Int.DownSouthWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            //if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-WEST
            point = parent.index + Direction3Int.DownWest;
            if (IsWalkable(point)) { SetNeighbor(point, parent, 1.414f); }
            if (IsClimbable(point)) { SetNeighbor(point, parent, 1.414f); }

            // BOTTOM-NORTH-WEST
            point = parent.index + Direction3Int.DownNorthWest;
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

            byte block;
            block = _world.data.Get(point);
            if (_world.blocks.IsSolid(block)) { return false; }     // the point must be an air block

            block = _world.data.Get(point + Direction3Int.Down);
            if (!_world.blocks.IsSolid(block)) { return false; }    // the point below must be solid

            block = _world.data.Get(point + Direction3Int.Up);
            if (_world.blocks.IsSolid(block)) { return false; }     // the point above must be air

            return true;
        }

        /// <summary>
        /// Test if the grid point is a valid navigation node
        /// </summary>
        private bool IsClimbable(Vector3Int point)
        {
            if (!_world.data.Contains(point)) { return false; }     // the point must be inside the world

            byte block;
            block = _world.data.Get(point);
            if (_world.blocks.IsSolid(block)) { return false; }     // the point must be an air block

            block = _world.data.Get(point + Direction3Int.Down);
            if (_world.blocks.IsSolid(block)) { return false; }     // the point below must be air

            block = _world.data.Get(point + Direction3Int.Up);
            if (_world.blocks.IsSolid(block)) { return false; }     // the point above must be air

            // climbable if any adjacent N,E,S,W block is solid
            block = _world.data.Get(point + Direction3Int.North);
            if (_world.blocks.IsSolid(block)) { return true; }

            block = _world.data.Get(point + Direction3Int.East);
            if (_world.blocks.IsSolid(block)) { return true; }

            block = _world.data.Get(point + Direction3Int.South);
            if (_world.blocks.IsSolid(block)) { return true; }

            block = _world.data.Get(point + Direction3Int.West);
            if (_world.blocks.IsSolid(block)) { return true; }

            // the point was not climbable
            return false;
        }
    }
}
