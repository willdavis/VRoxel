using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World _world;
    Pathfinder _pathfinder;
    Vector3 _structurePosition;

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
        Vector3Int index = new Vector3Int(x, y, z);

        // spawn a structure at the center of the world
        Vector3 position = WorldEditor.Get(_world, index);
        position += Vector3.down * 0.5f * _world.scale; // adjust to the floor
        _structurePosition = position;

        GameObject obj = Instantiate(structure, position, _world.transform.rotation) as GameObject;
        obj.transform.localScale = Vector3.one * _world.scale;
        obj.transform.parent = _world.transform;

        // generate nodes for the pathfinder
        // this method creates a flow field for a tower defence style game
        _pathfinder.BFS(index); // breadth first search

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
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            // calculate the adjusted hit position
            // and get the index point in the voxel grid
            Vector3 position = WorldEditor.Adjust(_world, hit, Cube.Point.Outside);
            Vector3Int index = WorldEditor.Get(_world, position);

            // draw the block cursor and snap its position to the grid
            Vector3 gridPosition = WorldEditor.Get(_world, index);
            cursor.Draw(_world, gridPosition, _world.scale / 2f);

            // left mouse click to add blocks
            if(Input.GetMouseButtonDown(0)){
                WorldEditor.Set(_world, index, 1);
            }

            // spawn a new NPC in the world
            if (Input.GetKeyDown(KeyCode.N))
            {
                // ensure the agent spawns on the ground
                int y = _world.terrain.GetHeight(index.x, index.z) + 1;
                Vector3Int floor = new Vector3Int(index.x, y, index.z);
                Vector3 newAgentPos = WorldEditor.Get(_world, floor);

                // spawn a new agent and set its destination to the goal
                NavAgent newAgent = _world.agents.Spawn(agent, newAgentPos);
                newAgent.pathfinder = _pathfinder;
                newAgent.destination = _structurePosition;
            }
        }

        // Remove NPCs if they have reached the goal
        for (int i = 0; i < _world.agents.all.Count; i++)
        {
            NavAgent agent = _world.agents.all[i];
            float radius = agent.range * 2.5f;
            float dist = Vector3.Distance(agent.transform.position, agent.destination);

            if (dist <= radius)
            {
                _world.agents.all.Remove(agent);
                GameObject.Destroy(agent.gameObject);
            }
        }
    }

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
