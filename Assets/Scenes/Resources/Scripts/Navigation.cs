using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Navigation;

public class Navigation : MonoBehaviour
{
    World _world;
    EditWorld _editor;
    Pathfinding _pathfinding;
    AgentManager _agents;
    JobHandle jobHandle;

    public int maxAgents = 1000;
    public KeyCode spawnAgent = KeyCode.N;

    void Awake()
    {
        _world = GetComponent<World>();
        _editor = GetComponent<EditWorld>();
        _pathfinding = GetComponent<Pathfinding>();
    }

    void Start()
    {
        NavAgent[] temp = new NavAgent[maxAgents];
        Transform[] transforms = new Transform[maxAgents];
        NavAgentPool.Instance.AddObjects(maxAgents);
        _agents = new AgentManager(maxAgents);

        // initialize all of the agents
        for (int i = 0; i < maxAgents; i++)
        {
            NavAgent agent = NavAgentPool.Instance.Get();
            agent.transform.parent = NavAgentPool.Instance.transform;
            _agents.all.Add(agent);

            Enemy enemy = agent.GetComponent<Enemy>();
            enemy.OnDeath.AddListener(Remove);

            temp[i] = agent;
            transforms[i] = agent.transform;
        }

        // return all of the agents back to the pool
        foreach (var agent in temp)
        {
            NavAgentPool.Instance.ReturnToPool(agent);
        }

        _agents.TransformAccess(transforms);
    }

    void OnDestroy()
    {
        _agents.Dispose();
    }

    void Update()
    {
        jobHandle.Complete();
        HandlePlayerInput();
        UpdateAgentPositions();
    }

    void HandlePlayerInput()
    {
        if (Input.GetKey(spawnAgent) && CanSpawnAt(_editor.currentIndex))
            Spawn(_editor.currentPosition);
    }

    void UpdateAgentPositions()
    {
        _agents.MoveAgents(Time.deltaTime);
        //jobHandle = _agents.MoveAgentsAsync(Time.deltaTime);
    }

    /// <summary>
    /// Test if an agent can be spawned at the voxel world coordinates
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

        agent.pathfinder = _pathfinding.pathfinder;
        agent.destination = _pathfinding.goalPostPosition;

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
