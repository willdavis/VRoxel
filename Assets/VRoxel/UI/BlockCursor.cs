using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCursor : MonoBehaviour
{
    LineRenderer _renderer;
    Vector3[] _cube = new Vector3[8];

    void Awake()
    {
        _renderer = GetComponent<LineRenderer>();
    }

    public void Draw(World world, Vector3 position, float scale)
    {
        _renderer.startColor = Color.yellow;
        _renderer.endColor = Color.yellow;
        _renderer.widthMultiplier = world.scale / 10f;
        Cube.Transform(position, scale, world.transform.rotation, ref _cube);
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        _renderer.SetPosition(0, _cube[0]);    // set 0
        _renderer.SetPosition(1, _cube[1]);    // set 0 -> 1
        _renderer.SetPosition(2, _cube[2]);    // set 1 -> 2
        _renderer.SetPosition(3, _cube[3]);    // set 2 -> 3
        _renderer.SetPosition(4, _cube[0]);    // set 3 -> 0
        _renderer.SetPosition(5, _cube[4]);    // set 0 -> 4
        _renderer.SetPosition(6, _cube[5]);    // set 4 -> 5
        _renderer.SetPosition(7, _cube[1]);    // set 5 -> 1
        _renderer.SetPosition(8, _cube[2]);    // set 1 -> 2
        _renderer.SetPosition(9, _cube[6]);    // set 2 -> 6
        _renderer.SetPosition(10, _cube[5]);   // set 6 -> 5
        _renderer.SetPosition(11, _cube[4]);   // set 5 -> 4
        _renderer.SetPosition(12, _cube[7]);   // set 4 -> 7
        _renderer.SetPosition(13, _cube[6]);   // set 7 -> 6
        _renderer.SetPosition(14, _cube[2]);   // set 6 -> 2
        _renderer.SetPosition(15, _cube[3]);   // set 2 -> 3
        _renderer.SetPosition(16, _cube[7]);   // set 3 -> 7
    }
}
