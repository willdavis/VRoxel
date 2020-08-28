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


    void Awake()
    {
        _world = GetComponent<World>();
        _blocks = GetComponent<BlockManager>();
        _heightMap = GetComponent<HeightMap>();
    }

    void Start()
    {
        _world.Initialize();
        GenerateTerrainData();

        MeshGenerator generator = new MeshGenerator(
            _world.data, _blocks, _world.scale
        );

        VRoxel.Core.Data.ChunkConfiguration configuration = ScriptableObject
                .CreateInstance("ChunkConfiguration") as VRoxel.Core.Data.ChunkConfiguration;

        configuration.collidable = true;
        configuration.scale = _world.scale;
        configuration.size = _world.chunkSize;
        configuration.material = _blocks.textureAtlas.material;

        _world.chunks.configuration = configuration;
        _world.chunks.meshGenerator = generator;
        _world.chunks.LoadAll();    // 4. initialize all chunks in the world

        _heightMap.voxels = _world.data.voxels;
        _heightMap.Initialize();    // 5. initialize the height map
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
                        _world.data.Set(point, grass);
                    else if (point.y >= height-3 && point.y <  height)
                        _world.data.Set(point, dirt);
                    else if (point.y <  height-3)
                        _world.data.Set(point, stone);
                }
            }
        }
    }
}
