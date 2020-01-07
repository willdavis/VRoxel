using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;
using VRoxel.Navigation;

public class Pathfinding : MonoBehaviour
{
    World _world;
    LoadWorld _loader;
    Pathfinder _pathfinder;
    List<GameObject> _pathfinderNodes;


    bool _stalePath = true;
    Vector3Int goalPostIndex = Vector3Int.zero;
    Vector3 goalPostPosition = Vector3.zero;


    [Header("Setting Goals")]
    public Vector2Int destination;
    public GameObject goalPost;
    public GameObject pathfinderNodePrefab;

    void Awake()
    {
        _world = GetComponent<World>();
        _loader = GetComponent<LoadWorld>();
        _pathfinder = new Pathfinder(_world);
        _pathfinderNodes = new List<GameObject>();
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
    }

    /// <summary>
    /// Update the goal post's position in the scene
    /// </summary>
    void MoveTheGoalPost()
    {
        // calculate the height of the terrain at the destination
        int height = _loader.terrain.GetHeight(
            destination.x, destination.y
        );

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
            DrawPathfindingNodes();
            _stalePath = false;
        }
    }

    /// <summary>
    /// Build a flow field for agents to navigate towards the goal post
    /// </summary>
    void CalculatePathfindingNodes()
    {
        _pathfinder.BFS(goalPostIndex);
        //_pathfinder.Dijkstra(goalPostIndex);
    }

    /// <summary>
    /// Draw each node including their direction towards the goal post
    /// </summary>
    void DrawPathfindingNodes()
    {
        foreach (GameObject item in _pathfinderNodes)
        {
            Object.Destroy(item);
        }
        _pathfinderNodes.Clear();

        foreach (Pathfinder.Node node in _pathfinder.nodes.Values)
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
}
