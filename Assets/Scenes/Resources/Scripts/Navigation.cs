using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;
using VRoxel.Navigation;

public class Navigation : MonoBehaviour
{
    World _world;
    EditWorld _editor;
    Pathfinding _pathfinding;
    AgentManager _agents;

    public KeyCode spawnAgent = KeyCode.N;

    [Header("Prefab Settings")]
    public NavAgent navAgentPrefab;

    void Awake()
    {
        _agents = new AgentManager();
        _world = GetComponent<World>();
        _editor = GetComponent<EditWorld>();
        _pathfinding = GetComponent<Pathfinding>();
    }

    void Update()
    {
        HandlePlayerInput();
        UpdateAgentPositions();
    }

    void HandlePlayerInput()
    {
        if (Input.GetKey(spawnAgent) && CanSpawnAt(_editor.currentIndex))
        {
            NavAgent newAgent = Spawn(navAgentPrefab, _editor.currentPosition);
            newAgent.pathfinder = _pathfinding.pathfinder;
            newAgent.destination = _pathfinding.goalPostPosition;
        }
    }

    void UpdateAgentPositions()
    {
        _agents.MoveAgents(Time.deltaTime);
        _agents.MoveAgentsAsync(Time.deltaTime);
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
    public NavAgent Spawn(NavAgent prefab, Vector3 position)
    {
        Quaternion rotation = _world.transform.rotation;
        NavAgent agent = UnityEngine.Object.Instantiate(prefab, position, rotation) as NavAgent;
        agent.transform.localScale = Vector3.one * _world.scale;
        agent.transform.parent = _world.transform;
        _agents.all.Add(agent);

        Enemy enemy = agent.GetComponent<Enemy>();
        enemy.OnDeath.AddListener(Remove);

        return agent;
    }

    /// <summary>
    /// Removes an agent from the scene
    /// </summary>
    public void Remove(Enemy enemy)
    {
        NavAgent agent = enemy.GetComponent<NavAgent>();

        _agents.all.Remove(agent);
        GameObject.Destroy(agent.gameObject);
    }
}
