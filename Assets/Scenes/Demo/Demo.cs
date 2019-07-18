using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World _world;

    public NavAgent prefab;
    public BlockCursor cursor;
    public Material material;
    public float textureSize = 0.25f;

    void Awake()
    {
        _world = GetComponent<World>();
    }

    void Start()
    {
        _world.Initialize();
        _world.blocks = BuildBlockManager();
        _world.Generate(_world.size, Vector3Int.zero);
        _world.chunks.Load(_world.chunks.max, Vector3Int.zero);
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
                _world.agents.Spawn(prefab, gridPosition);
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
