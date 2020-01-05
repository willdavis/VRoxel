using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;

public class EditWorld : MonoBehaviour
{
    [Header("Block Settings")]
    public byte blockType = 0;


    [Header("Cursor Settings")]
    public float size = 1;
    public bool snapToGrid = true;
    public BlockCursor.Shape shape = BlockCursor.Shape.Cuboid;
    public Cube.Point adjustHitPosition = Cube.Point.Outside;


    [Header("Cursor Prefab")]
    public BlockCursor cursor;

    World _world;
    RaycastHit _hit;
    Vector3Int _voxelIndex;
    Vector3 _voxelPosition;
    Vector3 _hitPosition;

    void Awake()
    {
        _world = GetComponent<World>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        if (Physics.Raycast (ray, out _hit))
        {
            CacheHitVoxelData();
            DrawCursor();
        }
    }

    /// <summary>
    /// Cache the scene position and index of the voxel that was hit
    /// </summary>
    void CacheHitVoxelData()
    {
        // The RaycastHit position will be on the face of a voxel.
        // We need to offset the position either inside or outside of the voxel.
        switch (adjustHitPosition)
        {
            case Cube.Point.Inside:     // used when changing blocks to air
                _hitPosition = WorldEditor.Adjust(_world, _hit, Cube.Point.Inside);
                break;
            case Cube.Point.Outside:    // used when adding blocks to air
                _hitPosition = WorldEditor.Adjust(_world, _hit, Cube.Point.Outside);
                break;
            default:
                break;
        }

        // find and cache the index of the voxel that was hit
        // this can be used later to get/set the voxel data at that position
        _voxelIndex = WorldEditor.Get(_world, _hitPosition);

        if (snapToGrid)
        {
            // calculate and cache the scene postion of the voxel index
            // this will keep the cursor "stuck" to the current voxel until the mouse moves to another
            _voxelPosition = WorldEditor.Get(_world, _voxelIndex);
        }
        else
        {
            // cache the scene postion of the hit point
            // this gives a more smooth motion to the cursor
            _voxelPosition = _hitPosition;
        }
    }

    /// <summary>
    /// Update the BlockCursor object to the current mouse position
    /// </summary>
    void DrawCursor()
    {
        switch (shape)
        {
            case BlockCursor.Shape.Cuboid:
                cursor.UpdateCuboid(_world, _voxelPosition, _voxelPosition, size);
                break;
            case BlockCursor.Shape.Spheroid:
                cursor.UpdateSpheroid(_world, _voxelPosition, _voxelPosition, size);
                break;
            default:
                Debug.Log("Shape not recognized");
                break;
        }
    }
}
