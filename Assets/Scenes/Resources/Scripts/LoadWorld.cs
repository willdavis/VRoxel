using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;
using VRoxel.Terrain;

public class LoadWorld : MonoBehaviour
{

    World _world;

    [HideInInspector]
    public Generator terrain;


    [Header("Terrain Settings")]
    public int seed = 1337;
    public float noise = 0.25f;
    public float scale = 25f;
    public float offset = 10f;


    [Header("Texture Settings")]
    public Material material;
    public float textureSize = 0.25f;


    void Awake()
    {
        _world = GetComponent<World>();
    }

    void Start()
    {
        _world.Initialize();        // 1. initialize the voxel grid
        BuildBlockManager();        // 2. initialize the different blocks
        GenerateTerrainData();      // 3. populate the voxel grid with data
        _world.chunks.LoadAll();    // 4. initialize all chunks in the world
    }

    void BuildBlockManager()
    {
        BlockManager blocks = new BlockManager();
        _world.blocks = blocks;

        // Assign Texture Settings to the BlockManager
        blocks.texture.material = material;
        blocks.texture.size = textureSize;

        // Create blocks and add textures
        Block air = new Block();
        air.index = 0;
        air.isSolid = false;

        Block grass = new Block();
        grass.index = 1;
        grass.isSolid = true;

        Block stone = new Block();
        stone.index = 2;
        stone.isSolid = true;

        Vector2 grassTop = new Vector2(0,15);
        Vector2 grassSide = new Vector2(2,15);
        Vector2 grassBottom = new Vector2(2,15);

        grass.textures.Add(Cube.Direction.Top, grassTop);
        grass.textures.Add(Cube.Direction.Bottom, grassBottom);
        grass.textures.Add(Cube.Direction.North, grassSide);
        grass.textures.Add(Cube.Direction.East, grassSide);
        grass.textures.Add(Cube.Direction.South, grassSide);
        grass.textures.Add(Cube.Direction.West, grassSide);

        Vector2 stoneTexture = new Vector2(1,15);

        stone.textures.Add(Cube.Direction.Top, stoneTexture);
        stone.textures.Add(Cube.Direction.Bottom, stoneTexture);
        stone.textures.Add(Cube.Direction.North, stoneTexture);
        stone.textures.Add(Cube.Direction.East, stoneTexture);
        stone.textures.Add(Cube.Direction.South, stoneTexture);
        stone.textures.Add(Cube.Direction.West, stoneTexture);

        // Add Blocks to the library
        blocks.library.Add(air.index, air);
        blocks.library.Add(grass.index, grass);
        blocks.library.Add(stone.index, stone);
    }

    void GenerateTerrainData()
    {
        int height;
        Vector3Int point = Vector3Int.zero;
        terrain = new Generator(seed, noise, scale, offset);

        for (int x = 0; x < _world.size.x; x++)
        {
            point.x = x;
            for (int z = 0; z < _world.size.z; z++)
            {
                point.z = z;
                height = terrain.GetHeight(point.x, point.z);              // get the terrain height at (x,z)
                for (int y = 0; y < _world.size.y; y++)
                {
                    point.y = y;
                    if (point.y == 0) { _world.data.Set(point, 1); }        // create a bottom layer
                    if (point.y <= height) { _world.data.Set(point, 1); }   // fill in the terrain
                }
            }
        }
    }
}
