using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    World _world;
    Pathfinder _pathfinder;
    List<GameObject> _pathNodes;
    Vector3 _goalPosition;
    Vector3Int _goalGridPosition;

    List<Turret> _turrets = new List<Turret>();

    // variables for the block cursor
    // for single or multiple blocks
    bool _draggingCursor;
    Vector3Int _start;
    Vector3 _startPosition;

    [Header("Prefab Settings")]
    public NavAgent agent;
    public GameObject nodePrefab;
    public GameObject goalPrefab;
    public Turret turretPrefab;
    public BlockCursor cursor;

    [Header("Input Settings")]
    public byte blockType = 1;
    public float radius = 8.5f;
    public int neighborhood = 1;
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
        _pathfinder = new Pathfinder(_world);
        _pathNodes = new List<GameObject>();
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
        _goalGridPosition = new Vector3Int(x, y, z);

        // spawn a structure at the center of the world
        _goalPosition = WorldEditor.Get(_world, _goalGridPosition);
        _goalPosition += Vector3.down * 0.5f * _world.scale; // adjust to the floor

        GameObject obj = Instantiate(goalPrefab, _goalPosition, _world.transform.rotation) as GameObject;
        obj.transform.localScale = Vector3.one * _world.scale;
        obj.transform.parent = _world.transform;

        // generate nodes for the pathfinder
        // this method creates a flow field for a tower defence style game
        //_pathfinder.BFS(_goalGridPosition); // breadth first search
        _pathfinder.Dijkstra(_goalGridPosition);

        // DEBUG: view the path nodes
        //DrawPathNodes();
    }

    void Update()
    {
        HandleUserInput();      // handle anything the user has input
        RemoveNPCsAtGoal();     // remove any NPCs that have reached the goal
        npcCount.text = npcCountString + _world.agents.all.Count;
        nodeCount.text = nodeCountString + _pathfinder.nodes.Count;
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
        for (int i = 0; i < _world.agents.all.Count; i++)
        {
            NavAgent agent = _world.agents.all[i];
            float dist = Vector3.Distance(agent.transform.position, agent.destination);

            if (dist <= agent.range * agent.range * 2f)
            {
                _world.agents.all.Remove(agent);
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

    /// <summary>
    /// Process any user input for this frame
    /// </summary>
    void HandleUserInput()
    {
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit))
        {
            // calculate the adjusted hit position
            // and get the index point in the voxel grid
            // and snap the index to its grid position in the scene
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

            Vector3Int index = WorldEditor.Get(_world, position);
            Vector3 gridPosition = WorldEditor.Get(_world, index);
            float halfScale = _world.scale / 2f;

            // draw the block cursor
            if(_draggingCursor) // draw a rectangle for the cursor
            {
                float offsetX = Mathf.Abs(_start.x - index.x);
                float offsetY = Mathf.Abs(_start.y - index.y);
                float offsetZ = Mathf.Abs(_start.z - index.z);

                Vector3 scale = new Vector3(
                    halfScale * offsetX + halfScale,
                    halfScale * offsetY + halfScale,
                    halfScale * offsetZ + halfScale
                );

                cursor.Draw(_world, _startPosition, gridPosition, scale);
            }
            else // draw a single block for the cursor
            {
                cursor.Draw(_world, gridPosition, halfScale);
            }

            // left mouse click to add blocks
            // click & drag to add multiple blocks
            if(Input.GetMouseButtonDown(0))
            {
                _start = index;
                _startPosition = gridPosition;
                _draggingCursor = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _draggingCursor = false;
                WorldEditor.Set(_world, _start, index, blockType);      // set the world data
                //_pathfinder.BFS(_goalGridPosition);              // rebuild pathfinding nodes
                _pathfinder.Dijkstra(_goalGridPosition);

                // DEBUG
                //DrawPathNodes();
            }

            // Key G - place a sphere of blocks in the world
            if (Input.GetKeyDown(KeyCode.G))
            {
                WorldEditor.Set(_world, index, radius, blockType);
            }

            // Key H - place a Moore neighborhood of blocks in the world
            if (Input.GetKeyDown(KeyCode.H))
            {
                WorldEditor.Set(_world, index, neighborhood, blockType);
            }

            // Key N - spawn a new NPC in the world
            if (Input.GetKey(KeyCode.N) && CanSpawnAt(index))
            {
                NavAgent newAgent = _world.agents.Spawn(agent, gridPosition);
                newAgent.pathfinder = _pathfinder;
                newAgent.destination = _goalPosition;
            }

            // Key T - spawn a turret in the world to shoot NPCs
            if (Input.GetKeyDown(KeyCode.T) && CanSpawnAt(index))
            {
                Turret turret = Instantiate(turretPrefab, gridPosition, _world.transform.rotation) as Turret;
                turret.transform.position += Vector3.down * 0.5f * _world.scale; // adjust to the floor
                turret.transform.localScale = Vector3.one * _world.scale;
                turret.transform.parent = _world.transform;

                // configure the turret
                turret.range = 10f * _world.scale;
                turret.targets = _world.agents.all;

                _turrets.Add(turret);
            }
        }
    }
}
