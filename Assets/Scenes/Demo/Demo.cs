using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    World world;
    public Material material;
    public float textureSize = 0.25f;

    void Awake()
    {
        world = GetComponent<World>();
    }

    void Start()
    {
        world.blocks.texture.material = material;
        world.blocks.texture.size = textureSize;

        // Create blocks and add textures
        Block air = new Block(); air.index = 0;
        Block stone = new Block(); stone.index = 1;

        Vector2 grassTop = new Vector2(0,15);
        Vector2 grassSide = new Vector2(2,15);
        Vector2 dirt = new Vector2(2,15);

        stone.textures.Add(Cube.Direction.Top, grassTop);
        stone.textures.Add(Cube.Direction.Bottom, dirt);
        stone.textures.Add(Cube.Direction.North, grassSide);
        stone.textures.Add(Cube.Direction.East, grassSide);
        stone.textures.Add(Cube.Direction.South, grassSide);
        stone.textures.Add(Cube.Direction.West, grassSide);

        // Add Blocks to the library
        world.blocks.library.Add(air.index, air);
        world.blocks.library.Add(stone.index, stone);

        world.Initialize();
        world.Generate(world.size, Vector3Int.zero);
        world.chunks.Load(world.chunks.max, Vector3Int.zero);
    }

    void Update()
    {
        
    }
}
