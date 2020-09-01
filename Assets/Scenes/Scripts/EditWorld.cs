using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Core.Data;
using VRoxel.Terrain;

[RequireComponent(typeof(World), typeof(BlockManager), typeof(HeightMap))]
public class EditWorld : MonoBehaviour
{
    /// <summary>
    /// The current type of block used to edit the world
    /// </summary>
    public BlockConfiguration block;


    [Header("Cursor Settings")]
    public float size = 1;
    public bool snapToGrid = true;
    public bool clickAndDrag = true;
    public BlockCursor.Shape shape = BlockCursor.Shape.Cuboid;
    public Cube.Point adjustHitPosition = Cube.Point.Outside;

    bool _isDragging = false;
    Vector3 _clickStart = Vector3.zero;

    [Header("Cursor Prefab")]
    public BlockCursor cursor;

    World _world;
    BlockManager _blockManager;

    RaycastHit _hit;
    Vector3 _hitPosition;

    HeightMap _heightMap;

    public JobHandle editHandle;
    public JobHandle heightMapHandle;

    [HideInInspector]
    public Vector3Int currentIndex;
    
    [HideInInspector]
    public Vector3 currentPosition;

    void Awake()
    {
        _world = GetComponent<World>();
        _heightMap = GetComponent<HeightMap>();
        _blockManager = GetComponent<BlockManager>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
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
            _clickStart = currentPosition;
            _isDragging = true;
        }

        if (Input.GetMouseButtonUp(0)) // left click - up
        {
            _isDragging = false;
            if (clickAndDrag) { EditRectangle(); }
            else { EditBlock(); }
        }
    }

    void EditBlock()
    {
        if (block == null) { return; }

        byte index = (byte)_blockManager.blocks.IndexOf(block);
        WorldEditor.Set(_world, currentPosition, index);

        _heightMap.Refresh();
        _world.data.OnEdit.Invoke(editHandle);
    }

    void EditRectangle()
    {
        if (block == null) { return; }
        if (!editHandle.IsCompleted) { editHandle.Complete(); }

        /// old version, still used for rendering
        byte index = (byte)_blockManager.blocks.IndexOf(block);
        WorldEditor.Set(_world, _clickStart, currentPosition, index);

        /// new version, for async pathfinding
        Vector3Int start = WorldEditor.Get(_world, _clickStart);
        Vector3Int end = WorldEditor.Get(_world, currentPosition);

        EditVoxelJob job = new EditVoxelJob()
        {
            size = new int3(_world.size.x, _world.size.y, _world.size.z),
            start = new int3(start.x, start.y, start.z),
            end = new int3(end.x, end.y, end.z),
            voxels = _world.data.voxels,
            block = index
        };

        editHandle = job.Schedule();
        heightMapHandle = _heightMap.Refresh(editHandle);
        _world.data.OnEdit.Invoke(heightMapHandle);
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
        currentIndex = WorldEditor.Get(_world, _hitPosition);

        if (snapToGrid)
        {
            // calculate and cache the scene postion of the voxel index
            // this will keep the cursor "stuck" to the current voxel until the mouse moves to another
            currentPosition = WorldEditor.Get(_world, currentIndex);
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
            case BlockCursor.Shape.Cuboid:
                if (clickAndDrag && _isDragging) { DrawRectangle(); }
                else { DrawCube(); }
                break;
            case BlockCursor.Shape.Spheroid:
                cursor.UpdateSpheroid(_world, currentPosition, currentPosition, size);
                break;
            default:
                Debug.Log("Shape not recognized");
                break;
        }
    }

    void DrawCube() { cursor.UpdateCuboid(_world, currentPosition, currentPosition, size); }
    void DrawRectangle() { cursor.UpdateCuboid(_world, _clickStart, currentPosition, size); }
}
