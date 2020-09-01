﻿using System.Collections.Generic;
using Core.Utilities;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using VRoxel.Core;
using VRoxel.Terrain;
using VRoxel.Navigation;
using VRoxel.Navigation.Data;

public class Navigation : MonoBehaviour
{
    bool initialized;

    JobHandle moveHandle;
    JobHandle updateHandle;

    [Header("References")]
    public World world;
    public HeightMap heightMap;
    public EditWorld editor;
    public NavAgentManager agentManager;

    [Header("Agent Settings")]
    public int maxAgents = 1000;
    public KeyCode spawnAgents = KeyCode.N;
    public bool autoSpawnAgents = true;
    public List<NavAgent> agentsToSpawn;

    [Header("Goal Settings")]
    public Vector2Int goalPosition;
    public GameObject goal;

    void Awake()
    {
        if (agentManager == null)
            agentManager = GetComponent<NavAgentManager>();

        if (heightMap == null)
            heightMap = GetComponent<HeightMap>();

        if (editor == null)
            editor = GetComponent<EditWorld>();

        if (world == null)
            world = GetComponent<World>();
    }

    void Start()
    {
        // configure the agent manager
        agentManager.spatialBucketSize = new Unity.Mathematics.int3(2,2,2);

        // movement
        agentManager.maxForce = 10f * world.scale;

        // collision
        agentManager.collisionForce = 6f * world.scale;
        agentManager.collisionRadius = 1.5f * world.scale;
        agentManager.maxCollisionDepth = 200;

        // queuing
        agentManager.brakeForce = 0.8f;
        agentManager.queueRadius = 1.5f * world.scale;
        agentManager.queueDistance = 1.5f * world.scale;
        agentManager.maxQueueDepth = 200;

        // avoidance
        agentManager.avoidForce = 4f * world.scale;
        agentManager.avoidRadius = 3f * world.scale;
        agentManager.avoidDistance = 8f * world.scale;
        agentManager.maxAvoidDepth = 200;

        NavAgent[] agents = new NavAgent[maxAgents];
        Dictionary<NavAgentArchetype, List<Transform>> transforms = new Dictionary<NavAgentArchetype, List<Transform>>();

        // initialize agent archetype transforms
        for (int a = 0; a < agentManager.archetypes.Count; a++)
            transforms[agentManager.archetypes[a]] = new List<Transform>();

        // initialize each agent
        int agentPartition = maxAgents / agentManager.archetypes.Count;
        for (int i = 0; i < maxAgents; i++)
        {
            int index = i / agentPartition;
            var agent = Poolable.TryGetPoolable<NavAgent>(
                agentsToSpawn[index].gameObject);

            agent.index = i;
            agents[i] = agent;

            Enemy enemy = agent.GetComponent<Enemy>();
            enemy.OnDeath.AddListener(Remove);

            NavAgentArchetype archetype = agent.configuration.archetype;
            if (!transforms.ContainsKey(archetype))
                transforms[archetype] = new List<Transform>();
            transforms[archetype].Add(agent.transform);
        }

        // re-pool the initialized agents
        foreach (var agent in agents)
        {
            Poolable.TryPool(agent.gameObject);
        }

        agentManager.Initialize(transforms);
    }

    void OnDestroy()
    {
        updateHandle.Complete();
        moveHandle.Complete();
    }

    void Update()
    {
        if (!initialized) { Initialize(); }
        UpdateGoalPostPosition();

        if (autoSpawnAgents)
            SpawnAgents(1);
        else if (Input.GetKey(spawnAgents) && CanSpawnAt(editor.currentIndex))
            Spawn(editor.currentPosition);

        moveHandle = agentManager.MoveAgents(Time.deltaTime, updateHandle);
    }

    void LateUpdate()
    {
        updateHandle.Complete();
        moveHandle.Complete();
    }

    void Initialize()
    {
        initialized = true;
        goal.transform.position = GetGoalScenePosition();

        world.data.OnEdit.AddListener(UpdatePathfindingAsync);
        agentManager.UpdateFlowFields(GetGoalGridPosition(), updateHandle).Complete();
    }

    void UpdatePathfindingAsync(JobHandle handle)
    {
        handle.Complete(); // ensure the height map is updated
        moveHandle.Complete(); // ensure agents are done moving

        Vector3Int goal = GetGoalGridPosition();
        updateHandle = agentManager.UpdateFlowFields(goal, handle);
    }

    void UpdateGoalPostPosition()
    {
        Vector3 goalPosition = GetGoalScenePosition();
        Vector3Int goalGridPoint = GetGoalGridPosition();

        if (goal.transform.position == goalPosition) { return; }
        goal.transform.position = goalPosition;

        updateHandle.Complete();
        updateHandle = agentManager.UpdateFlowFields(goalGridPoint, updateHandle);
    }

    /// <summary>
    /// Calculates the voxel coordinates for the goal
    /// </summary>
    Vector3Int GetGoalGridPosition()
    {
        int x = goalPosition.x;
        int z = goalPosition.y;
        int y = heightMap.Read(x,z) + 1;
        return new Vector3Int(x,y,z);
    }

    /// <summary>
    /// Calculates the scene position for the goal
    /// </summary>
    Vector3 GetGoalScenePosition()
    {
        Vector3Int grid = GetGoalGridPosition();
        Vector3 position = WorldEditor.Get(world, grid);
        position += Vector3.down * 0.5f * world.scale;
        return position;
    }

    /// <summary>
    /// Test if an agent can be spawned at the voxel coordinates
    /// </summary>
    public bool CanSpawnAt(Vector3Int index)
    {
        return world.data.Get(index) == 0                      // the block must be air
            && world.data.Get(index + Vector3Int.down) != 0;   // the block below must be solid
    }

    /// <summary>
    /// Adds a new agent to the scene
    /// </summary>
    public NavAgent Spawn(Vector3 position)
    {
        int index = Random.Range(0, agentsToSpawn.Count-1);
        NavAgent agent = Poolable.TryGetPoolable<NavAgent>(
                agentsToSpawn[index].gameObject);

        agent.transform.localScale = Vector3.one * world.scale;
        agent.transform.rotation = world.transform.rotation;
        agent.transform.position = position;
        agent.gameObject.SetActive(true);

        NativeSlice<bool> slice = agentManager.activeAgents.Slice(agent.index, 1);
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
            world.size.x / 2,
            world.size.z / 2
        );

        for (int i = 0; i < count; i++)
        {
            // choose a random (x,z) position inside a circle
            Vector2 randomXZ = Random.insideUnitCircle;
            randomXZ *= world.size.x / 2;
            randomXZ += center;

            // update the agents grid position and
            // convert the grid position to world space
            grid.x = (int)randomXZ.x; grid.z = (int)randomXZ.y;
            grid.y = heightMap.Read(grid.x, grid.z) + 1;
            position = WorldEditor.Get(world, grid);

            // spawn the new enemy agent
            int index = Random.Range(0, agentsToSpawn.Count-1);
            NavAgent agent = Poolable.TryGetPoolable<NavAgent>(
                    agentsToSpawn[index].gameObject);

            agent.transform.localScale = Vector3.one * world.scale;
            agent.transform.rotation = world.transform.rotation;
            agent.transform.position = position;
            agent.gameObject.SetActive(true);

            // activate the agent's navigation behaviors
            NativeSlice<bool> slice = agentManager.activeAgents.Slice(agent.index, 1);
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
        Poolable.TryPool(agent.gameObject);

        NativeSlice<bool> slice = agentManager.activeAgents.Slice(agent.index, 1);
        ActivateAgents activation = new ActivateAgents()
        {
            status = false,
            agents = slice
        };
        activation.Schedule(1,1).Complete();
    }
}