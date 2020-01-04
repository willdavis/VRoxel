using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRoxel.Core;

public class EditWorld : MonoBehaviour
{
    [Header("Cursor Settings")]
    public float size = 0;
    public bool snapToGrid = true;

    [Header("Cursor Prefab")]
    public BlockCursor cursor;

    World _world;
    RaycastHit _hit;
    Vector3Int _voxelIndex;
    Vector3 _voxelPosition;

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
    /// Cache the scene and grid position of the voxel that was hit
    /// </summary>
    void CacheHitVoxelData()
    {
        // offset the hit point outside the face that was hit (best for adding blocks)
        Vector3 outsideTheHitBlock = WorldEditor.Adjust(_world, _hit, Cube.Point.Outside);

        // cache the voxel grid index that was hit
        // this can be used later to get/set the voxel data at the index
        _voxelIndex = WorldEditor.Get(_world, outsideTheHitBlock);

        if (snapToGrid)
        {
            // calculate and cache the scene postion of the voxel index
            // this will keep the cursor "stuck" to the current voxel until the mouse hits to another
            _voxelPosition = WorldEditor.Get(_world, _voxelIndex);
        }
        else
        {
            // cache the scene postion of the hit point
            // this gives a more smooth motion to the cursor
            _voxelPosition = outsideTheHitBlock;
        }
    }

    /// <summary>
    /// Update the BlockCursor object to the current mouse position
    /// </summary>
    void DrawCursor()
    {
        cursor.UpdateCuboid(_world, _voxelPosition, _voxelPosition, size);
    }
}
