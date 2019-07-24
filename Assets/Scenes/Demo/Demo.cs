﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World _world;
    Pathfinder _pathfinder;
    Vector3 _structurePosition;
    Vector3Int _structureGridPosition;


    // variables for the block cursor
    // for single or multiple blocks
    bool _draggingCursor;
    Vector3Int _start;
    Vector3 _startPosition;

    public NavAgent agent;
    public GameObject nodePrefab;
    public GameObject structure;
    public BlockCursor cursor;
    public Material material;
    public float textureSize = 0.25f;

    void Awake()
    {
        _world = GetComponent<World>();
        _pathfinder = new Pathfinder(_world);
    }

    void Start()
    {
        _world.Initialize();
        _world.blocks = BuildBlockManager();
        _world.Generate(_world.size, Vector3Int.zero);
        _world.chunks.Load(_world.chunks.max, Vector3Int.zero);

        // get the index point for the center of the world at terrain level
        int x = Mathf.FloorToInt(_world.size.x / 2f);
        int z = Mathf.FloorToInt(_world.size.z / 2f);
        int y = _world.terrain.GetHeight(x, z) + 1;
        _structureGridPosition = new Vector3Int(x, y, z);

        // spawn a structure at the center of the world
        Vector3 _structurePosition = WorldEditor.Get(_world, _structureGridPosition);
        _structurePosition += Vector3.down * 0.5f * _world.scale; // adjust to the floor

        GameObject obj = Instantiate(structure, _structurePosition, _world.transform.rotation) as GameObject;
        obj.transform.localScale = Vector3.one * _world.scale;
        obj.transform.parent = _world.transform;

        // generate nodes for the pathfinder
        // this method creates a flow field for a tower defence style game
        _pathfinder.BFS(_structureGridPosition); // breadth first search

        // DEBUG: view the path nodes
        /*
        foreach (Pathfinder.Node node in _pathfinder._closed.Values)
        {
            Vector3 node_pos = WorldEditor.Get(_world, node.index);
            Vector3 parent_pos = WorldEditor.Get(_world, node.parent);
            GameObject debugNode = Instantiate(nodePrefab, node_pos, _world.transform.rotation) as GameObject;
            debugNode.transform.LookAt(parent_pos);
        }
        */
    }

    void Update()
    {
        RemoveNPCsAtGoal();

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            // calculate the adjusted hit position
            // and get the index point in the voxel grid
            // and snap the index to its grid position in the scene
            Vector3 position = WorldEditor.Adjust(_world, hit, Cube.Point.Outside);
            Vector3Int index = WorldEditor.Get(_world, position);
            Vector3 gridPosition = WorldEditor.Get(_world, index);
            float halfScale = _world.scale / 2f;

            // draw the block cursor
            if(_draggingCursor) // draw a rectangle for the cursor
            {
                float offsetX = Mathf.Abs(_start.x - index.x);
                float offsetY = Mathf.Abs(_start.y - index.y);
                float offsetZ = Mathf.Abs(_start.z - index.z);

                Vector3 scale = new Vector3(
                    halfScale * offsetX + halfScale,
                    halfScale * offsetY + halfScale,
                    halfScale * offsetZ + halfScale
                );

                cursor.Draw(_world, _startPosition, gridPosition, scale);
            }
            else // draw a single block for the cursor
            {
                cursor.Draw(_world, gridPosition, halfScale);
            }
            

            // left mouse click to add blocks
            // click & drag to add multiple blocks
            if(Input.GetMouseButtonDown(0))
            {
                _start = index;
                _startPosition = gridPosition;
                _draggingCursor = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                WorldEditor.Set(_world, _start, index, 1);  // set the world data
                _pathfinder.BFS(_structureGridPosition);    // rebuild pathfinding nodes
                _draggingCursor = false;
            }

            // spawn a new NPC in the world
            if (Input.GetKeyDown(KeyCode.N))
            {
                NavAgent newAgent = _world.agents.Spawn(agent, gridPosition);
                newAgent.pathfinder = _pathfinder;
                newAgent.destination = _structurePosition;
            }
        }
    }

    /// <summary>
    /// Remove any NPCs that have reached the goal structure.
    /// </summary>
    void RemoveNPCsAtGoal()
    {
        for (int i = 0; i < _world.agents.all.Count; i++)
        {
            NavAgent agent = _world.agents.all[i];
            float dist = Vector3.Distance(agent.transform.position, agent.destination);

            if (dist <= 2f)
            {
                _world.agents.all.Remove(agent);
                GameObject.Destroy(agent.gameObject);
            }
        }
    }

    /// <summary>
    /// An example on how to build a BlockManager for this demo world
    /// </summary>
    BlockManager BuildBlockManager()
    {
        BlockManager manager = new BlockManager();

        manager.texture.material = material;
        manager.texture.size = textureSize;

        // Create blocks and add textures
        Block air = new Block(); air.index = 0;
        Block grass = new Block(); grass.index = 1;

        Vector2 grassTop = new Vector2(0,15);
        Vector2 grassSide = new Vector2(2,15);
        Vector2 grassBottom = new Vector2(2,15);

        grass.textures.Add(Cube.Direction.Top, grassTop);
        grass.textures.Add(Cube.Direction.Bottom, grassBottom);
        grass.textures.Add(Cube.Direction.North, grassSide);
        grass.textures.Add(Cube.Direction.East, grassSide);
        grass.textures.Add(Cube.Direction.South, grassSide);
        grass.textures.Add(Cube.Direction.West, grassSide);

        // Add Blocks to the library
        manager.library.Add(air.index, air);
        manager.library.Add(grass.index, grass);

        return manager;
    }
}
