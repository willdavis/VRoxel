using UnityEngine;
using VRoxel.Core;
using VRoxel.Terrain;

public class LoadWorld : MonoBehaviour
{
    public World world;
    public HeightMap heightMap;
    public BlockManager blockManager;

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
        if (world == null)
            world = GetComponent<World>();
        if (blockManager == null)
            blockManager = GetComponent<BlockManager>();
        if (heightMap == null)
            heightMap = GetComponent<HeightMap>();
    }

    void Update()
    {
        if (!initialized)
            Initialize();
    }

    void Initialize()
    {
        initialized = true;

        world.Initialize();
        world.chunkManager.meshGenerator = new MeshGenerator(world);
        world.chunkManager.LoadAll(); // initialize all chunks in the world

        GenerateTerrainData();

        heightMap.voxels = world.data.voxels;
        heightMap.Initialize(); // initialize the height map
        heightMap.Refresh().Complete();
    }

    void GenerateTerrainData()
    {
        int height;
        Vector3Int point = Vector3Int.zero;
        terrain = new Generator(seed, noise, scale, offset);

        byte dirt  = blockManager.IndexOf("dirt");
        byte grass = blockManager.IndexOf("grass");
        byte stone = blockManager.IndexOf("stone");

        for (int x = 0; x < world.size.x; x++)
        {
            point.x = x;
            for (int z = 0; z < world.size.z; z++)
            {
                point.z = z;
                height = terrain.GetHeight(point.x, point.z);
                for (int y = 0; y < world.size.y; y++)
                {
                    point.y = y;
                    if (point.y == height)
                        world.Write(point, grass);
                    else if (point.y >= height-3 && point.y <  height)
                        world.Write(point, dirt);
                    else if (point.y <  height-3)
                        world.Write(point, stone);
                }
            }
        }
    }
}
