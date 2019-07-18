using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    public Vector3 destination;
    public float range = 0.5f;
    public float speed = 5.0f;

    void Start()
    {
        destination = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance > range)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, destination, step);
        }
    }
}
