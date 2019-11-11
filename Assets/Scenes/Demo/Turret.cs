using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Navigation;

public class Turret : MonoBehaviour
{
    [Header("Settings")]
    public Transform fireFrom;      // the point to shoot from
    public LineRenderer bullet;     // a lazer beam

    [Header("Runtime")]
    public List<NavAgent> targets;  // all potential targets in the world
    public float range;             // the distance the turret can shoot

    void Update()
    {
        bullet.SetPosition(0, fireFrom.transform.position); // reset the lazer
        bullet.SetPosition(1, fireFrom.transform.position); // reset the lazer

        if (targets.Count == 0) { return; } // no targets available

        NavAgent target = FindTarget();
        if (target == null) { return; }     // no targets in range

        bullet.SetPosition(0, fireFrom.transform.position); // set the lazer on the target
        bullet.SetPosition(1, target.transform.position);   // set the lazer on the target

        targets.Remove(target);         // kill the target instantly
        Destroy(target.gameObject);     // and remove the gameObject
    }

    NavAgent FindTarget()
    {
        NavAgent target = null;
        float shortest = float.MaxValue;
        for (int i = 0; i < targets.Count; i++)
        {
            NavAgent agent = targets[i];
            float dist = Vector3.Distance(
                agent.transform.position,
                fireFrom.transform.position
            );

            if (dist <= range && dist <= shortest)
            {
                shortest = dist;
                target = agent;
            }
        }
        return target;
    }
}
