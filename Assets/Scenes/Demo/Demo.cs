using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using VRoxel.Core;
using VRoxel.Navigation;

public class Demo : MonoBehaviour
{
    World _world;
    Terrain _terrain;
    AgentManager _agents;
    Pathfinder _pathfinder;
    List<GameObject> _pathNodes;

    Vector3Int goalPoint;           // the point in the voxel grid the AI is trying to get to
    Vector3 goalPosition;           // the world scene position the AI is trying to get to
    List<Turret> turrets = new List<Turret>();

    bool draggingCursor;            // is the Player clicking and dragging the cursor?
    Vector3Int clickStartPoint;     // the point in the voxel grid the Player started the click + drag at
    Vector3 clickStartPosition;     // the world scene position the Player started the click + drag at



    [Header("Prefab Settings")]
    public NavAgent agent;
    public GameObject nodePrefab;
    public GameObject goalPrefab;
    public Turret turretPrefab;
    public BlockCursor cursor;

    [Header("Input Settings")]
    public BlockCursor.Shape blockShape = BlockCursor.Shape.Cuboid;
    public byte blockType = 1;
    public float blockRadius = 6.5f;
    public int blockSize = 1;
    public Cube.Point hitAdjustment = Cube.Point.Outside;

    [Header("UI Settings")]
    public Text npcCount;
    private string npcCountString = "NPC Count: ";
    public Text nodeCount;
    private string nodeCountString = "Pathfinding Nodes: ";

    [Header("Texture Settings")]
    public Material material;
    public float textureSize = 0.25f;

    void Awake()
    {
        _world = GetComponent<World>();
        _agents = new AgentManager(_world);
        _pathfinder = new Pathfinder(_world);
        _pathNodes = new List<GameObject>();
        _terrain = new Terrain(0, 0.25f, 25f, 10f);
    }

    void Start()
    {
        _world.Initialize();
        _world.blocks = BuildBlockManager();
        Generate(_world.size, Vector3Int.zero);
        _world.chunks.Load(_world.chunks.max, Vector3Int.zero);

        // get the index point for the center of the world at terrain level
        int x = Mathf.FloorToInt(_world.size.x / 2f);
        int z = Mathf.FloorToInt(_world.size.z / 2f);
        int y = _terrain.GetHeight(x, z) + 1;
        goalPoint = new Vector3Int(x, y, z);

        // spawn a structure at the center of the world
        goalPosition = WorldEditor.Get(_world, goalPoint);
        goalPosition += Vector3.down * 0.5f * _world.scale; // adjust to the floor

        GameObject obj = Instantiate(goalPrefab, goalPosition, _world.transform.rotation) as GameObject;
        obj.transform.localScale = Vector3.one * _world.scale;
        obj.transform.parent = _world.transform;

        // generate nodes for the pathfinder
        // this method creates a flow field for a tower defence style game
        //_pathfinder.BFS(goalPoint); // breadth first search
        // - or -
        _pathfinder.Dijkstra(goalPoint);
    }

    void Update()
    {
        HandleUserInput();      // handle anything the user has input
        RemoveNPCsAtGoal();     // remove any NPCs that have reached the goal
        npcCount.text = npcCountString + _agents.all.Count;
        nodeCount.text = nodeCountString + _pathfinder.nodes.Count;
    }

    /// <summary>
    /// Generate world data within the given bounds.
    /// Any points outside the world will be skipped.
    /// </summary>
    /// <param name="size">The number of voxels to generate</param>
    /// <param name="offset">The offset from the world origin</param>
    public void Generate(Vector3Int size, Vector3Int offset)
    {
        int terrain;
        Vector3Int point = Vector3Int.zero;
        for (int x = 0; x < size.x; x++)
        {
            point.x = x + offset.x;
            for (int z = 0; z < size.z; z++)
            {
                point.z = z + offset.z;
                terrain = _terrain.GetHeight(point.x, point.z);
                for (int y = 0; y < size.y; y++)
                {
                    point.y = y + offset.y;
                    if (!_world.data.Contains(point)) { continue; }
                    if (point.y == 0) { _world.data.Set(point, 1); }
                    if (point.y <= terrain) { _world.data.Set(point, 1); }
                }
            }
        }
    }

    void DrawPathNodes()
    {
        foreach (GameObject item in _pathNodes)
        {
            Object.Destroy(item);
        }
        _pathNodes.Clear();

        foreach (Pathfinder.Node node in _pathfinder.nodes.Values)
        {
            Vector3 node_pos = WorldEditor.Get(_world, node.index);
            Vector3 parent_pos = WorldEditor.Get(_world, node.parent);
            GameObject debugNode = Instantiate(nodePrefab, node_pos, _world.transform.rotation) as GameObject;
            debugNode.transform.LookAt(parent_pos);
            _pathNodes.Add(debugNode);
        }
    }

    /// <summary>
    /// Test if the NPC can be spawned at a world index
    /// </summary>
    bool CanSpawnAt(Vector3Int index)
    {
        return _world.data.Get(index) == 0                      // the block must be air
            && _world.data.Get(index + Vector3Int.down) != 0;   // the block below must be solid
    }

    /// <summary>
    /// Remove any NPCs that have reached the goal structure.
    /// </summary>
    void RemoveNPCsAtGoal()
    {
        for (int i = 0; i < _agents.all.Count; i++)
        {
            NavAgent agent = _agents.all[i];
            float dist = Vector3.Distance(agent.transform.position, agent.destination);

            if (dist <= agent.range * agent.range * 2f)
            {
                _agents.all.Remove(agent);
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
        manager.library.Add(air.index, air);
        manager.library.Add(grass.index, grass);
        manager.library.Add(stone.index, stone);

        return manager;
    }

    Vector3 GetAdjustedHit(RaycastHit hit)
    {
        Vector3 position = Vector3.zero;
        switch (hitAdjustment)
        {
            case Cube.Point.Inside: // used to replace blocks
                position = WorldEditor.Adjust(_world, hit, Cube.Point.Inside);
                break;
            case Cube.Point.Outside: // used to add blocks
                position = WorldEditor.Adjust(_world, hit, Cube.Point.Outside);
                break;
        }
        return position;
    }

    void DrawCursor(Vector3 position)
    {
        if(draggingCursor)
        {
            if (blockShape == BlockCursor.Shape.Cuboid)
            {
                cursor.UpdateCuboid(_world, clickStartPosition, position);
            }
        }
        else
        {
            float scale = 2f * (float)blockSize + 1f;
            if (blockShape == BlockCursor.Shape.Cuboid)
            {
                cursor.UpdateCuboid(_world, position, position, scale);
            }
            else
            {
                cursor.UpdateSpheroid(_world, position, position, blockRadius);
            }
        }
    }

    void HandleMouseInput(Vector3Int hitPoint, Vector3 hitPosition)
    {
        if(Input.GetMouseButtonDown(0) && blockShape == BlockCursor.Shape.Cuboid) // left click
        {
            clickStartPoint = hitPoint;
            clickStartPosition = hitPosition;
            draggingCursor = true;
        }
        if (Input.GetMouseButtonUp(0) && blockShape == BlockCursor.Shape.Cuboid) // left click
        {
            WorldEditor.Set(_world, clickStartPoint, hitPoint, blockType);
            draggingCursor = false;

            // update AI pathfinding
            //_pathfinder.BFS(goalPoint);
            // - or -
            _pathfinder.Dijkstra(goalPoint);
        }
        if (Input.GetMouseButtonUp(0) && blockShape == BlockCursor.Shape.Spheroid) // left click
        {
            WorldEditor.Set(_world, hitPoint, blockRadius / 2f, blockType);

            // update AI pathfinding
            //_pathfinder.BFS(goalPoint);
            // - or -
            _pathfinder.Dijkstra(goalPoint);
        }
    }

    /// <summary>
    /// Process any user input for this frame
    /// </summary>
    void HandleUserInput()
    {
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            Vector3 adjustedHit = GetAdjustedHit(hit);
            Vector3Int hitPoint = WorldEditor.Get(_world, adjustedHit);     // the voxel grid point that was hit
            Vector3 hitPosition = WorldEditor.Get(_world, hitPoint);        // the world scene position for the voxel point

            HandleMouseInput(hitPoint, hitPosition);
            DrawCursor(hitPosition);

            // Key H - place a Moore neighborhood of blocks in the world
            if (Input.GetKeyDown(KeyCode.H))
            {
                WorldEditor.Set(_world, hitPoint, blockSize, blockType);
                _pathfinder.Dijkstra(goalPoint);
            }

            // Key N - spawn a new NPC in the world
            if (Input.GetKey(KeyCode.N) && CanSpawnAt(hitPoint))
            {
                NavAgent newAgent = _agents.Spawn(agent, hitPosition);
                newAgent.pathfinder = _pathfinder;
                newAgent.destination = goalPosition;
            }

            // Key T - spawn a turret in the world to shoot NPCs
            if (Input.GetKeyDown(KeyCode.T) && CanSpawnAt(hitPoint))
            {
                Turret turret = Instantiate(turretPrefab, hitPosition, _world.transform.rotation) as Turret;
                turret.transform.position += Vector3.down * 0.5f * _world.scale; // adjust to the floor
                turret.transform.localScale = Vector3.one * _world.scale;
                turret.transform.parent = _world.transform;

                // configure the turret
                turret.range = 10f * _world.scale;
                turret.targets = _agents.all;
                turrets.Add(turret);
            }
        }
    }
}
