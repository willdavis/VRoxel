using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;

public class Pathfinding : MonoBehaviour
{
    World _world;
    LoadWorld _loader;

    [Header("Setting Goals")]
    public Vector2Int destination;
    public GameObject goalPost;

    void Awake()
    {
        _world = GetComponent<World>();
        _loader = GetComponent<LoadWorld>();
    }

    void Start()
    {
        // scale the goal post to the size of the world
        goalPost.transform.localScale = Vector3.one * _world.scale;
    }

    void Update()
    {
        MoveTheGoalPost();
    }

    /// <summary>
    /// Update the goal post position if the destination changes
    /// </summary>
    void MoveTheGoalPost()
    {
        int height = _loader.terrain.GetHeight(
            destination.x, destination.y
        );

        Vector3Int goalPoint = new Vector3Int(
            destination.x, height, destination.y
        );

        Vector3 goalPosition = WorldEditor.Get(_world, goalPoint);
        goalPosition += Vector3.down * 0.5f * _world.scale;
        goalPost.transform.position = goalPosition;
    }
}
