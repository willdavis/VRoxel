using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Settings")]
    public Transform fireFrom;
    public LineRenderer bullet;

    [Header("Runtime")]
    public List<NavAgent> targets;
    public float range;         // the distance the turret can shoot
    public float dps;           // damage per second
}
