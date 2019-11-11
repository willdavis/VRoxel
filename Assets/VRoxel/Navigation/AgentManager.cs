using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;

public class AgentManager
{
    private World _world;
    private List<NavAgent> _agents;

    public AgentManager(World world)
    {
        _world = world;
        _agents = new List<NavAgent>();
    }

    /// <summary>
    /// Returns a list of all managed agents
    /// </summary>
    public List<NavAgent> all { get { return _agents; } }

    /// <summary>
    /// Adds a new NavAgent (NPC) to the World
    /// </summary>
    public NavAgent Spawn(NavAgent prefab, Vector3 position)
    {
        Quaternion rotation = _world.transform.rotation;
        NavAgent agent = UnityEngine.Object.Instantiate(prefab, position, rotation) as NavAgent;
        agent.transform.localScale = Vector3.one * _world.scale;
        agent.transform.parent = _world.transform;

        _agents.Add(agent);
        return agent;
    }
}
