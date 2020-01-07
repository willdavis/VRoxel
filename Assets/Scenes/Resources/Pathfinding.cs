using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;

public class Pathfinding : MonoBehaviour
{
    World _world;

    [Header("Setting Goals")]
    public Vector2Int destination;
    public GameObject goalPost;

    void Awake()
    {
        _world = GetComponent<World>();
    }
}
