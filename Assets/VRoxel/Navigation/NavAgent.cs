using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Navigation
{
    public class NavAgent : MonoBehaviour
    {
        private Vector3 _destination;
        private Vector3 _waypoint;

        public float range = 0.5f;
        public float speed = 5.0f;
        public Pathfinder pathfinder;

        public Vector3 destination
        {
            get { return _destination; }
            set { _destination = value; NextWaypoint(); }
        }

        void Update()
        {
            float distance = Vector3.Distance(transform.position, destination);
            if (distance <= range * range) { return; } // agent is at the destination

            // move towards the next waypoint in the path
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _waypoint, step);

            distance = Vector3.Distance(transform.position, _waypoint);
            if (distance <= range * range) { NextWaypoint(); } // agent is at the waypoint
        }

        public void NextWaypoint()
        {
            pathfinder.NextWaypoint(transform.position, ref _waypoint);
        }
    }
}
