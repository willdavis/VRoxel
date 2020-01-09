using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;
using VRoxel.Navigation;

public class Pathfinding : MonoBehaviour
{
    World _world;
    LoadWorld _loader;
    List<GameObject> _pathfinderNodes;
    bool _stalePath = true;


    [HideInInspector]
    public Pathfinder pathfinder;

    [HideInInspector]
    public Vector3Int goalPostIndex = Vector3Int.zero;

    [HideInInspector]
    public Vector3 goalPostPosition = Vector3.zero;


    public Vector2Int destination;
    public bool drawDebugNodes = false;


    [Header("Prefab Settings")]
    public GameObject goalPost;
    public GameObject pathfinderNodePrefab;

    void Awake()
    {
        _world = GetComponent<World>();
        _loader = GetComponent<LoadWorld>();
        _pathfinderNodes = new List<GameObject>();

        pathfinder = new Pathfinder(_world);
    }

    void Start()
    {
        // scale the goal post to the size of the world
        goalPost.transform.localScale = Vector3.one * _world.scale;
    }

    void Update()
    {
        MoveTheGoalPost();
        UpdatePathfindingNodes();
        HandlePlayerInput();
    }

    /// <summary>
    /// Handle any input from the player
    /// </summary>
    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonUp(0)) { _stalePath = true; }
    }

    /// <summary>
    /// Update the goal post's position in the scene
    /// </summary>
    void MoveTheGoalPost()
    {
        int height = 0;

        // scan from bedrock to the first air block
        for (int i = 0; i < _world.size.y; i++)
        {
            bool found = _world.data.Get(destination.x, i, destination.y) == 0;
            if (found) { height = i; break; }
        }

        // cache where the goal post is in voxel world coordinates
        goalPostIndex.y = height;
        goalPostIndex.x = destination.x;
        goalPostIndex.z = destination.y;

        // update the goal posts position in the scene
        goalPostPosition = WorldEditor.Get(_world, goalPostIndex);
        goalPostPosition += Vector3.down * 0.5f * _world.scale;
        goalPost.transform.position = goalPostPosition;
    }


    /// <summary>
    /// Rebuild pathfinding nodes only if the data is stale
    /// </summary>
    void UpdatePathfindingNodes()
    {
        if (_stalePath)
        {
            CalculatePathfindingNodes();

            if (drawDebugNodes) { DrawPathfindingNodes(); }
            else { ClearPathfindingNodes(); }

            _stalePath = false;
        }
    }

    /// <summary>
    /// Build a flow field for agents to navigate towards the goal post
    /// </summary>
    void CalculatePathfindingNodes()
    {
        pathfinder.BFS(goalPostIndex);
        //pathfinder.Dijkstra(goalPostIndex);
    }

    /// <summary>
    /// Draw each node including their direction towards the goal post
    /// </summary>
    void DrawPathfindingNodes()
    {
        ClearPathfindingNodes();

        foreach (Pathfinder.Node node in pathfinder.nodes.Values)
        {
            Vector3 nodePosition = WorldEditor.Get(_world, node.index);
            Vector3 parentPosition = WorldEditor.Get(_world, node.parent);
            GameObject newNode = Instantiate(
                pathfinderNodePrefab, nodePosition, _world.transform.rotation
            ) as GameObject;

            newNode.transform.LookAt(parentPosition);
            _pathfinderNodes.Add(newNode);
        }
    }

    void ClearPathfindingNodes()
    {
        foreach (GameObject item in _pathfinderNodes)
        {
            Object.Destroy(item);
        }
        _pathfinderNodes.Clear();
    }
}
