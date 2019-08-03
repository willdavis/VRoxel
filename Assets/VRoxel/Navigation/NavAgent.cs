using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    private Vector3 _destination;
    private List<Vector3> _path = new List<Vector3>();

    public float range = 0.5f;
    public float speed = 5.0f;
    public Pathfinder pathfinder;

    public Vector3 destination
    {
        get { return _destination; }
        set { _destination = value; BuildPath(); }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance <= range * range) { return; } // agent is at the destination
        if (_path.Count == 0) { return; } // no valid path was found

        // move towards the next waypoint in the path
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _path[0], step);

        distance = Vector3.Distance(transform.position, _path[0]);
        if (distance <= range * range) { BuildPath(); } // agent is at the waypoint
    }

    public void BuildPath()
    {
        _path.Clear();
        pathfinder.PathFrom(transform.position, ref _path);
    }
}
