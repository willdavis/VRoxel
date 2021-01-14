using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Core.Data;
using VRoxel.Terrain;

public class EditWorld : MonoBehaviour
{
    public World world;
    public HeightMap heightMap;
    public BlockManager blockManager;

    [Header("Block Settings")]

    /// <summary>
    /// The current type of block used to edit the world
    /// </summary>
    [Tooltip("The block to set when modifying the world")]
    public BlockConfiguration block;

    /// <summary>
    /// The blocks to ignore when modifying the world
    /// </summary>
    [Tooltip("These blocks will be ignored when modifying the world")]
    public List<BlockConfiguration> blocksToIgnore;
    protected NativeArray<byte> m_blocksToIgnore;
    protected int m_ignoreCount;


    [Header("Cursor Settings")]

    [Tooltip("The reference to the block cursor in the scene")]
    public BlockCursor cursor;

    [Tooltip("Choose the shape of how you want to edit the world")]
    public BlockCursor.Shape cursorShape = BlockCursor.Shape.Rectangle;

    [Tooltip("Moves the cursor position either inside or outside the block that was hit")]
    public Cube.Point adjustHitPosition = Cube.Point.Outside;

    public float sphereRadius = 2.5f;
    public bool snapToGrid = true;
    public bool clickAndDrag = true;

    bool _isDragging = false;
    Vector3 _clickStart = Vector3.zero;

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

        world.modified.AddListener(UpdateHeightMap);
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

    void OnDestroy()
    {
        if (m_blocksToIgnore.IsCreated)
            m_blocksToIgnore.Dispose();
    }

    /// <summary>
    /// Lazy load a native array of blocks to ignore for background jobs
    /// </summary>
    void LazyLoadReferences()
    {
        int ignoreCount = blocksToIgnore.Count;
        if (m_blocksToIgnore.IsCreated && m_ignoreCount == ignoreCount)
            return; // nothing has changed

        if (m_blocksToIgnore.IsCreated && m_ignoreCount != ignoreCount)
            m_blocksToIgnore.Dispose(); // clear any existing data

        m_ignoreCount = ignoreCount;
        m_blocksToIgnore = new NativeArray<byte>(
            ignoreCount, Allocator.Persistent);

        // update native array with current blocks to ignore
        for (int i = 0; i < ignoreCount; i++)
        {
            if (!world.blockManager.blocks.Contains(blocksToIgnore[i]))
                continue; // skip if not in the library

            m_blocksToIgnore[i] = (byte)world.blockManager
                .blocks.IndexOf(blocksToIgnore[i]);
        }
    }

    /// <summary>
    /// Respond to mouse input from the player
    /// </summary>
    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0)) // left mouse button - down
        {
            switch (cursorShape)
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
        else if (Input.GetMouseButtonUp(0)) // left mouse button - up
        {
            switch (cursorShape)
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
        else if (Input.GetMouseButton(0) && clickAndDrag) // left mouse button - hold
        {
            switch (cursorShape)
            {
                case BlockCursor.Shape.Rectangle:
                    break;
                case BlockCursor.Shape.Sphere:
                    EditSphere();
                    break;
            }
        }
    }

    /// <summary>
    /// Refreshes the height map after the world has been modified
    /// </summary>
    void UpdateHeightMap(JobHandle handle)
    {
        heightMapHandle = heightMap.Refresh(handle);
    }

    /// <summary>
    /// Modifies a single voxel block in the world
    /// </summary>
    void EditBlock()
    {
        if (block == null) { return; }
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetBlock(world, currentPosition, index);
    }

    /// <summary>
    /// Modifies all voxel blocks within a sphere
    /// </summary>
    void EditSphere()
    {
        if (block == null) { return; }

        LazyLoadReferences();
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetSphere(world, currentPosition, sphereRadius,
            index, ref editHandle, m_blocksToIgnore);
    }

    /// <summary>
    /// Modifies all voxel blocks within a rectangle
    /// </summary>
    void EditRectangle()
    {
        if (block == null) { return; }

        LazyLoadReferences();
        byte index = (byte)blockManager.blocks.IndexOf(block);
        WorldEditor.SetRectangle(world, currentPosition, _clickStart,
            index, ref editHandle, m_blocksToIgnore);
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
        switch (cursorShape)
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
