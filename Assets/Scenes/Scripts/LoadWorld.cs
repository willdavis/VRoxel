using UnityEngine;
using VRoxel.Core;
using VRoxel.Terrain;

[RequireComponent(typeof(World), typeof(BlockManager), typeof(HeightMap))]
public class LoadWorld : MonoBehaviour
{
    World _world;
    BlockManager _blocks;
    HeightMap _heightMap;

    [HideInInspector]
    public Generator terrain;


    [Header("Terrain Settings")]
    public int seed = 1337;
    public float noise = 0.25f;
    public float scale = 25f;
    public float offset = 10f;

    bool initialized;

    void Awake()
    {
        _world = GetComponent<World>();
        _blocks = GetComponent<BlockManager>();
        _heightMap = GetComponent<HeightMap>();
    }

    void Update()
    {
        if (!initialized)
            Initialize();
    }

    void Initialize()
    {
        initialized = true;

        _world.Initialize();
        _world.chunkManager.meshGenerator = new MeshGenerator(_world);
        _world.chunkManager.LoadAll(); // initialize all chunks in the world

        GenerateTerrainData();

        _heightMap.voxels = _world.data.voxels;
        _heightMap.Initialize(); // initialize the height map
        _heightMap.Refresh().Complete();
    }

    void GenerateTerrainData()
    {
        int height;
        Vector3Int point = Vector3Int.zero;
        terrain = new Generator(seed, noise, scale, offset);

        byte dirt  = _blocks.IndexOf("dirt");
        byte grass = _blocks.IndexOf("grass");
        byte stone = _blocks.IndexOf("stone");

        for (int x = 0; x < _world.size.x; x++)
        {
            point.x = x;
            for (int z = 0; z < _world.size.z; z++)
            {
                point.z = z;
                height = terrain.GetHeight(point.x, point.z);
                for (int y = 0; y < _world.size.y; y++)
                {
                    point.y = y;
                    if (point.y == height)
                        _world.Write(point, grass);
                    else if (point.y >= height-3 && point.y <  height)
                        _world.Write(point, dirt);
                    else if (point.y <  height-3)
                        _world.Write(point, stone);
                }
            }
        }
    }
}
