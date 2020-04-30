using UnityEngine;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Navigation;

public class Navigation : MonoBehaviour
{
    World _world;
    EditWorld _editor;
    AgentManager _agents;

    JobHandle moveHandle;
    JobHandle updateHandle;

    public int maxAgents = 1000;
    public KeyCode spawnAgent = KeyCode.N;

    [Header("Goal Settings")]
    public Vector2Int goalPosition;
    public GameObject goal;

    void Awake()
    {
        _world = GetComponent<World>();
        _editor = GetComponent<EditWorld>();
    }

    void Start()
    {
        NavAgent[] agents = new NavAgent[maxAgents];
        Transform[] transforms = new Transform[maxAgents];

        // initialize the object pool and agent manager
        NavAgentPool.Instance.AddObjects(maxAgents);
        _agents = new AgentManager(_world, maxAgents);

        // configure the agent manager
        _agents.spatialBucketSize = new Unity.Mathematics.int3(4,4,4);
        _agents.maxSpeed = 4f * _world.scale;
        _agents.turnSpeed = 4f * _world.scale;
        _agents.height = 2;
        _agents.mass = 1f;

        // movement
        _agents.moveForce = 10f * _world.scale;

        // queuing
        _agents.brakeForce = 0.8f;
        _agents.queueRadius = 1f * _world.scale;
        _agents.queueDistance = 1f * _world.scale;

        // avoidance
        _agents.avoidForce = 6f * _world.scale;
        _agents.avoidRadius = 4f * _world.scale;
        _agents.avoidDistance = 4f * _world.scale;


        // initialize all of the agents
        for (int i = 0; i < maxAgents; i++)
        {
            NavAgent agent = NavAgentPool.Instance.Get();
            Enemy enemy = agent.GetComponent<Enemy>();
            enemy.OnDeath.AddListener(Remove);

            agents[i] = agent;
            transforms[i] = agent.transform;
        }
        foreach (var agent in agents)
        {
            NavAgentPool.Instance.ReturnToPool(agent);
        }

        // configure the goal post position and scale
        goal.transform.position = GetGoalScenePosition();

        // configure the flow field and initialize it
        _agents.TransformAccess(transforms);
        _world.data.OnEdit.AddListener(UpdatePathfindingAsync);
        _agents.UpdateFlowField(GetGoalGridPosition(), updateHandle).Complete();
    }

    void OnDestroy()
    {
        updateHandle.Complete();
        moveHandle.Complete();

        _agents.Dispose();
    }

    void Update()
    {
        UpdateGoalPostPosition();

        SpawnAgents(1);
        //if (Input.GetKey(spawnAgent) && CanSpawnAt(_editor.currentIndex))
        //    Spawn(_editor.currentPosition);

        moveHandle = _agents.MoveAgentsAsync(Time.deltaTime);
    }

    void LateUpdate()
    {
        updateHandle.Complete();
        moveHandle.Complete();
    }

    void UpdatePathfindingAsync(JobHandle handle)
    {
        Vector3Int goal = GetGoalGridPosition();
        updateHandle = _agents.UpdateFlowField(goal, handle);
    }

    void UpdateGoalPostPosition()
    {
        Vector3 goalPosition = GetGoalScenePosition();
        Vector3Int goalGridPoint = GetGoalGridPosition();

        if (goal.transform.position == goalPosition) { return; }
        goal.transform.position = goalPosition;

        updateHandle.Complete();
        updateHandle = _agents.UpdateFlowField(goalGridPoint, updateHandle);
    }

    /// <summary>
    /// Calculates the voxel coordinates for the goal
    /// </summary>
    Vector3Int GetGoalGridPosition()
    {
        int y = _world.size.y - 1;
        int x = goalPosition.x;
        int z = goalPosition.y;

        // scan bottom to top for the first air block
        for (int i = 0; i < _world.size.y; i++)
        {
            bool found = _world.data.Get(x, i, z) == 0;
            if (found) { y = i - 1; break; }
        }

        return new Vector3Int(x,y,z);
    }

    /// <summary>
    /// Calculates the scene position for the goal
    /// </summary>
    Vector3 GetGoalScenePosition()
    {
        Vector3Int grid = GetGoalGridPosition();
        Vector3 position = WorldEditor.Get(_world, grid);
        position += Vector3.down * 0.5f * _world.scale;
        return position;
    }

    /// <summary>
    /// Test if an agent can be spawned at the voxel coordinates
    /// </summary>
    public bool CanSpawnAt(Vector3Int index)
    {
        return _world.data.Get(index) == 0                      // the block must be air
            && _world.data.Get(index + Vector3Int.down) != 0;   // the block below must be solid
    }

    /// <summary>
    /// Adds a new agent to the scene
    /// </summary>
    public NavAgent Spawn(Vector3 position)
    {
        NavAgent agent = NavAgentPool.Instance.Get();
        agent.transform.localScale = Vector3.one * _world.scale;
        agent.transform.rotation = _world.transform.rotation;
        agent.transform.position = position;
        agent.gameObject.SetActive(true);
        return agent;
    }

    public void SpawnAgents(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // position the enemy on the board
            Vector3 position = Vector3.zero;
            Vector3Int point = Vector3Int.zero;

            Vector2 center = new Vector2(
                _world.size.x / 2,
                _world.size.z / 2
            );
            Vector2 randomXZ = Random.insideUnitCircle;
            randomXZ *= _world.size.x / 2;
            randomXZ += center;

            point.x = (int)randomXZ.x;
            point.z = (int)randomXZ.y;
            point.y = _world.size.y;

            // find the terrain height
            for (int j = 0; j < _world.size.y; j++)
            {
                if (_world.data.Get(point.x, j, point.z) == 0)
                {
                    point.y = j;
                    break;
                }
            }

            // convert from voxel space to scene space
            position = WorldEditor.Get(_world, point);

            // spawn the new enemy
            NavAgent agent = NavAgentPool.Instance.Get();
            agent.transform.localScale = Vector3.one * _world.scale;
            agent.transform.rotation = _world.transform.rotation;
            agent.transform.position = position;
            agent.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Removes an agent from the scene
    /// </summary>
    public void Remove(Enemy enemy)
    {
        NavAgent agent = enemy.GetComponent<NavAgent>();
        NavAgentPool.Instance.ReturnToPool(agent);
    }
}
