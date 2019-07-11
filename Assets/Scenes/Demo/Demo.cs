using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World _world;

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
            Vector3 position = WorldEditor.Adjust(_world, hit, Cube.Point.Outside);
            Vector3Int index = WorldEditor.Get(_world, position);
            Vector3 adjusted = WorldEditor.Get(_world, index);
            adjusted += Vector3.one * 0.5f * _world.scale;
            cursor.Draw(_world, adjusted, _world.scale / 2f);
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
