using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager
{
    private World _world;

    public AgentManager(World world)
    {
        _world = world;
    }

    public NavAgent Spawn(NavAgent prefab, Vector3 position)
    {
        Quaternion rotation = _world.transform.rotation;
        NavAgent agent = UnityEngine.Object.Instantiate(prefab, position, rotation) as NavAgent;
        agent.transform.localScale = Vector3.one * _world.scale;
        agent.transform.parent = _world.transform;
        return agent;
    }
}
