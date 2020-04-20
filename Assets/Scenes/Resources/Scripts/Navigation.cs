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
        _agents.spatialBucketSize = new Unity.Mathematics.int3(1,1,1);
        _agents.agentRadius = 1f;
        _agents.agentHeight = 2;
        _agents.agentSpeed = 1f;
        _agents.agentTurnSpeed = 2f;

        // initialize all of the agents
        for (int i = 0; i < maxAgents; i++)
        {
            NavAgent agent = NavAgentPool.Instance.Get();
            agent.transform.parent = NavAgentPool.Instance.transform;
            _agents.all.Add(agent);

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
        goal.transform.localScale = Vector3.one * _world.scale;
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

        if (Input.GetKey(spawnAgent) && CanSpawnAt(_editor.currentIndex))
            Spawn(_editor.currentPosition);

        moveHandle.Complete();
        moveHandle = _agents.MoveAgentsAsync(Time.deltaTime);
    }

    void UpdatePathfindingAsync(JobHandle handle)
    {
        Vector3Int goal = GetGoalGridPosition();

        updateHandle.Complete();
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

    /// <summary>
    /// Removes an agent from the scene
    /// </summary>
    public void Remove(Enemy enemy)
    {
        NavAgent agent = enemy.GetComponent<NavAgent>();
        NavAgentPool.Instance.ReturnToPool(agent);
    }
}
