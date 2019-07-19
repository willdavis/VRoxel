using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    private Vector3 _destination;
    private Stack<Vector3> _path = new Stack<Vector3>();

    public float range = 0.5f;
    public float speed = 5.0f;

    public Vector3 destination
    {
        get { return _destination; }
        set { _destination = value; _path.Clear(); }
    }

    void Start()
    {
        _destination = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance <= range * range) { return; } // agent is at the destination
        if (_path.Count == 0) { BuildPath(); } // build a path to the destination

        // move towards the next waypoint in the path
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _path.Peek(), step);

        distance = Vector3.Distance(transform.position, _path.Peek());
        if (distance <= range * range) { _path.Pop(); } // agent is at the waypoint
    }

    void BuildPath()
    {
        _path.Push(_destination);
        _path.Push(Vector3.Lerp(transform.position, _destination, 0.5f) + (Vector3.up * 2.5f));
    }
}
