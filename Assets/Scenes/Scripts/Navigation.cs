using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using VRoxel.Core;
using VRoxel.Terrain;
using VRoxel.Navigation;
using VRoxel.Navigation.Data;

public class Navigation : MonoBehaviour
{
    World _world;
    EditWorld _editor;
    HeightMap _heightMap;
    NavAgentManager _agents;

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
        _heightMap = GetComponent<HeightMap>();
        _agents = GetComponent<NavAgentManager>();
    }

    void Start()
    {
        // initialize the object pool and agent manager
        NavAgentPool.Instance.AddObjects(maxAgents);

        // configure the agent manager
        _agents.spatialBucketSize = new Unity.Mathematics.int3(2,2,2);

        // movement
        _agents.maxForce = 10f * _world.scale;

        // collision
        _agents.collisionForce = 6f * _world.scale;
        _agents.collisionRadius = 1.5f * _world.scale;
        _agents.maxCollisionDepth = 200;

        // queuing
        _agents.brakeForce = 0.8f;
        _agents.queueRadius = 1.5f * _world.scale;
        _agents.queueDistance = 1.5f * _world.scale;
        _agents.maxQueueDepth = 200;

        // avoidance
        _agents.avoidForce = 4f * _world.scale;
        _agents.avoidRadius = 3f * _world.scale;
        _agents.avoidDistance = 8f * _world.scale;
        _agents.maxAvoidDepth = 200;

        NavAgent[] agents = new NavAgent[maxAgents];
        Dictionary<NavAgentArchetype, List<Transform>> transforms = new Dictionary<NavAgentArchetype, List<Transform>>();

        // initialize all of the agents
        for (int i = 0; i < maxAgents; i++)
        {
            NavAgent agent = NavAgentPool.Instance.Get();
            agent.index = i;

            Enemy enemy = agent.GetComponent<Enemy>();
            enemy.OnDeath.AddListener(Remove);

            NavAgentArchetype archetype = agent.configuration.archetype;
            if (!transforms.ContainsKey(archetype))
                transforms[archetype] = new List<Transform>();
            transforms[archetype].Add(agent.transform);

            agents[i] = agent;
        }
        foreach (var agent in agents)
        {
            NavAgentPool.Instance.ReturnToPool(agent);
        }

        // configure the goal post position and scale
        goal.transform.position = GetGoalScenePosition();

        // configure the flow field and initialize it
        _agents.Initialize(_world, transforms);
        _world.data.OnEdit.AddListener(UpdatePathfindingAsync);
        _agents.UpdateFlowFields(GetGoalGridPosition(), updateHandle).Complete();
    }

    void OnDestroy()
    {
        updateHandle.Complete();
        moveHandle.Complete();
    }

    void Update()
    {
        UpdateGoalPostPosition();

        SpawnAgents(1);
        //if (Input.GetKey(spawnAgent) && CanSpawnAt(_editor.currentIndex))
        //    Spawn(_editor.currentPosition);

        moveHandle = _agents.MoveAgents(Time.deltaTime, updateHandle);
    }

    void LateUpdate()
    {
        updateHandle.Complete();
        moveHandle.Complete();
    }

    void UpdatePathfindingAsync(JobHandle handle)
    {
        Vector3Int goal = GetGoalGridPosition();
        updateHandle = _agents.UpdateFlowFields(goal, handle);
    }

    void UpdateGoalPostPosition()
    {
        Vector3 goalPosition = GetGoalScenePosition();
        Vector3Int goalGridPoint = GetGoalGridPosition();

        if (goal.transform.position == goalPosition) { return; }
        goal.transform.position = goalPosition;

        updateHandle.Complete();
        updateHandle = _agents.UpdateFlowFields(goalGridPoint, updateHandle);
    }

    /// <summary>
    /// Calculates the voxel coordinates for the goal
    /// </summary>
    Vector3Int GetGoalGridPosition()
    {
        int x = goalPosition.x;
        int z = goalPosition.y;
        int y = _heightMap.Read(x,z) + 1;
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

        NativeSlice<bool> slice = _agents.activeAgents.Slice(agent.index, 1);
        ActivateAgents activation = new ActivateAgents()
        {
            status = true,
            agents = slice
        };
        activation.Schedule(1,1).Complete();

        return agent;
    }

    /// <summary>
    /// Adds multiple agents randomly in the scene
    /// </summary>
    public void SpawnAgents(int count)
    {
        Vector3 position = Vector3.zero;
        Vector3Int grid = Vector3Int.zero;
        Vector2 center = new Vector2(
            _world.size.x / 2,
            _world.size.z / 2
        );

        for (int i = 0; i < count; i++)
        {
            // choose a random (x,z) position inside a circle
            Vector2 randomXZ = Random.insideUnitCircle;
            randomXZ *= _world.size.x / 2;
            randomXZ += center;

            // update the agents grid position and
            // convert the grid position to world space
            grid.x = (int)randomXZ.x; grid.z = (int)randomXZ.y;
            grid.y = _heightMap.Read(grid.x, grid.z) + 1;
            position = WorldEditor.Get(_world, grid);

            // spawn the new enemy agent
            NavAgent agent = NavAgentPool.Instance.Get();
            agent.transform.localScale = Vector3.one * _world.scale;
            agent.transform.rotation = _world.transform.rotation;
            agent.transform.position = position;
            agent.gameObject.SetActive(true);

            // activate the agent's navigation behaviors
            NativeSlice<bool> slice = _agents.activeAgents.Slice(agent.index, 1);
            ActivateAgents activation = new ActivateAgents()
            {
                status = true,
                agents = slice
            };
            activation.Schedule(1,1).Complete();
        }
    }

    /// <summary>
    /// Removes an agent from the scene
    /// </summary>
    public void Remove(Enemy enemy)
    {
        NavAgent agent = enemy.GetComponent<NavAgent>();
        NavAgentPool.Instance.ReturnToPool(agent);

        NativeSlice<bool> slice = _agents.activeAgents.Slice(agent.index, 1);
        ActivateAgents activation = new ActivateAgents()
        {
            status = false,
            agents = slice
        };
        activation.Schedule(1,1).Complete();
    }
}
