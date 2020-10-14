using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Core.Data;
using VRoxel.Core.Chunks;
using VRoxel.Terrain;

public class EditWorld : MonoBehaviour
{
    public World world;
    public HeightMap heightMap;
    public BlockManager blockManager;

    /// <summary>
    /// The current type of block used to edit the world
    /// </summary>
    public BlockConfiguration block;

    /// <summary>
    /// The maximum number of chunks that can be refreshed each frame
    /// </summary>
    [Min(1)]
    [Tooltip("Limit the number of chunks that can be refreshed each frame")]
    public int refreshPerFrame = 1;


    [Header("Cursor Settings")]
    public BlockCursor.Shape shape = BlockCursor.Shape.Rectangle;
    public float sphereRadius = 2.5f;
    public bool snapToGrid = true;
    public bool clickAndDrag = true;
    public Cube.Point adjustHitPosition = Cube.Point.Outside;

    bool _isDragging = false;
    Vector3 _clickStart = Vector3.zero;

    [Header("Cursor Prefab")]
    public BlockCursor cursor;

    RaycastHit _hit;
    Vector3 _hitPosition;

    public JobHandle editHandle;
    public JobHandle heightMapHandle;

    [HideInInspector]
    public Vector3Int currentIndex;
    
    [HideInInspector]
    public Vector3 currentPosition;

    void Awake()
    {
        if (world == null)
            world = GetComponent<World>();
        if (heightMap == null)
            heightMap = GetComponent<HeightMap>();
        if (blockManager == null)
            blockManager = GetComponent<BlockManager>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast (ray, out _hit))
        {
            CacheHitVoxelData();
            HandlePlayerInput();
            DrawCursor();
        }
    }

    void LateUpdate()
    {
        editHandle.Complete();
        heightMapHandle.Complete();
    }

    /// <summary>
    /// Respond to mouse input from the player
    /// </summary>
    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0)) // left click - down
        {
            switch (shape)
            {
                case BlockCursor.Shape.Rectangle:
                    _clickStart = currentPosition;
                    _isDragging = true;
                    break;
                case BlockCursor.Shape.Sphere:
                    EditSphere();
                    break;
            }
        }

        if (Input.GetMouseButtonUp(0)) // left click - up
        {
            switch (shape)
            {
                case BlockCursor.Shape.Rectangle:
                    _isDragging = false;
                    if (clickAndDrag) { EditRectangle(); }
                    else { EditBlock(); }
                    break;
                case BlockCursor.Shape.Sphere:
                    break;
            }
        }
    }

    void EditBlock()
    {
        if (block == null) { return; }
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetBlock(world, currentPosition, index);

        heightMap.Refresh();
        world.data.OnEdit.Invoke(editHandle);
    }

    void EditSphere()
    {
        if (block == null) { return; }
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetSphere(world, currentPosition,
            sphereRadius, index, ref editHandle);

        // notify listeners
        heightMapHandle = heightMap.Refresh(editHandle);
        world.data.OnEdit.Invoke(heightMapHandle);
    }

    void EditRectangle()
    {
        if (block == null) { return; }
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetRectangle(world, currentPosition,
            _clickStart, index, ref editHandle);

        // notify listeners
        heightMapHandle = heightMap.Refresh(editHandle);
        world.data.OnEdit.Invoke(heightMapHandle);
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
            case Cube.Point.Inside:     // used when changing existing blocks
                _hitPosition = world.AdjustRaycastHit(_hit, Cube.Point.Inside);
                break;
            case Cube.Point.Outside:    // used when adding new blocks to air
                _hitPosition = world.AdjustRaycastHit(_hit, Cube.Point.Outside);
                break;
            default:
                break;
        }

        // find and cache the index of the voxel that was hit
        // this can be used later to get/set the voxel data at that position
        currentIndex = world.SceneToGrid(_hitPosition);

        if (snapToGrid)
        {
            // calculate and cache the scene postion of the voxel index
            // this will keep the cursor "stuck" to the current voxel until the mouse moves to another one
            currentPosition = world.GridToScene(currentIndex);
        }
        else
        {
            // cache the scene postion of the hit point
            // this gives a more smooth motion to the cursor
            currentPosition = _hitPosition;
        }
    }

    /// <summary>
    /// Update the BlockCursor object to the current mouse position
    /// </summary>
    void DrawCursor()
    {
        switch (shape)
        {
            case BlockCursor.Shape.Rectangle:
                if (clickAndDrag && _isDragging) { DrawRectangle(); }
                else { DrawCube(); }
                break;
            case BlockCursor.Shape.Sphere:
                cursor.UpdateSphere(world, currentPosition, currentPosition, sphereRadius * 2f);
                break;
            default:
                Debug.Log("Shape not recognized");
                break;
        }
    }

    void DrawCube() { cursor.UpdateRectangle(world, currentPosition, currentPosition); }
    void DrawRectangle() { cursor.UpdateRectangle(world, _clickStart, currentPosition); }
}
