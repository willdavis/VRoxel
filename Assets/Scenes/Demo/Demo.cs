using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World _world;
    Pathfinder _pathfinder;

    public NavAgent agent;
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

        // get the index point for the center of the world at the terrain level
        int x = Mathf.FloorToInt(_world.size.x / 2f);
        int z = Mathf.FloorToInt(_world.size.z / 2f);
        int y = _world.terrain.GetHeight(x, z) + 1;
        Vector3Int point = new Vector3Int(x, y, z);

        // spawn a structure at the center of the world
        Vector3 position = WorldEditor.Get(_world, point);
        GameObject obj = Instantiate(structure, position, _world.transform.rotation) as GameObject;
        obj.transform.localScale = Vector3.one * _world.scale;
        obj.transform.parent = _world.transform;

        // build a shared pathfinder and generate nodes to the structure
        // this is for basic Tower Defence mechanics
        //_pathfinder.GenerateNodesAround(point);
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
                _world.agents.Spawn(agent, gridPosition);
            }

            // set the destination for all NPCs
            if (Input.GetKeyDown(KeyCode.M))
            {
                foreach (NavAgent agent in _world.agents.all)
                {
                    agent.destination = gridPosition;
                }
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
