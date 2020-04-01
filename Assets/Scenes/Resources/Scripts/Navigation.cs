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
        RemoveAgentsAtDestination();
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
    }

    /// <summary>
    /// Adds a new NavAgent (NPC) to the World
    /// </summary>
    public NavAgent Spawn(NavAgent prefab, Vector3 position)
    {
        Quaternion rotation = _world.transform.rotation;
        NavAgent agent = UnityEngine.Object.Instantiate(prefab, position, rotation) as NavAgent;
        agent.transform.localScale = Vector3.one * _world.scale;
        agent.transform.parent = _world.transform;
        _agents.all.Add(agent);
        return agent;
    }

    /// <summary>
    /// Test if an agent can be spawned at the voxel world coordinates
    /// </summary>
    bool CanSpawnAt(Vector3Int index)
    {
        return _world.data.Get(index) == 0                      // the block must be air
            && _world.data.Get(index + Vector3Int.down) != 0;   // the block below must be solid
    }

    /// <summary>
    /// Remove any agents that have reached their goal
    /// </summary>
    void RemoveAgentsAtDestination()
    {
        for (int i = 0; i < _agents.all.Count; i++)
        {
            NavAgent agent = _agents.all[i];
            float dist = Vector3.Distance(agent.transform.position, agent.destination);

            if (dist <= agent.radius * agent.radius * 2f)
            {
                _agents.all.Remove(agent);
                GameObject.Destroy(agent.gameObject);
            }
        }
    }
}
