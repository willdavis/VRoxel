using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using VRoxel.Core;
using VRoxel.Core.Data;
using VRoxel.Core.Chunks;
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
    List<Chunk> m_editChunks;

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
        m_editChunks = new List<Chunk>();
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
        editHandle.Complete();

        byte index = (byte)_blockManager.blocks.IndexOf(block);
        Vector3Int start = WorldEditor.Get(_world, _clickStart);
        Vector3Int end = WorldEditor.Get(_world, currentPosition);

        // find the chunks that the rectangle intersects
        // and schedule jobs to update their voxel data

        Vector3Int chunkMin = Vector3Int.zero;
        Vector3Int chunkDelta = Vector3Int.zero;
        Vector3Int chunkEnd   = _world.chunks.IndexFrom(end);
        Vector3Int chunkStart = _world.chunks.IndexFrom(start);
        Vector3Int chunkSize = _world.chunks.configuration.size;

        // calculate min and delta of the rectangle so the
        // start and end positions orientation will not matter

        chunkDelta.x = Mathf.Abs(chunkEnd.x - chunkStart.x) + 1;
        chunkDelta.y = Mathf.Abs(chunkEnd.y - chunkStart.y) + 1;
        chunkDelta.z = Mathf.Abs(chunkEnd.z - chunkStart.z) + 1;

        chunkMin.x = Mathf.Min(chunkStart.x, chunkEnd.x);
        chunkMin.y = Mathf.Min(chunkStart.y, chunkEnd.y);
        chunkMin.z = Mathf.Min(chunkStart.z, chunkEnd.z);

        Chunk chunk;
        int jobIndex = 0;
        Vector3Int chunkIndex = Vector3Int.zero;
        int chunkCount = chunkDelta.x * chunkDelta.y * chunkDelta.z;
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(chunkCount, Allocator.Temp);

        for (int x = chunkMin.x; x < chunkMin.x + chunkDelta.x; x++)
        {
            chunkIndex.x = x;
            for (int z = chunkMin.z; z < chunkMin.z + chunkDelta.z; z++)
            {
                chunkIndex.z = z;
                for (int y = chunkMin.y; y < chunkMin.y + chunkDelta.y; y++)
                {
                    chunkIndex.y = y;
                    chunk = _world.chunks
                        .Get(chunkIndex);
                    m_editChunks.Add(chunk);

                    ModifyRectangle job = new ModifyRectangle();
                    job.chunkOffset = new int3(chunk.offset.x, chunk.offset.y, chunk.offset.z);
                    job.chunkSize = new int3(chunkSize.x, chunkSize.y, chunkSize.z);
                    job.start = new int3(start.x, start.y, start.z);
                    job.end = new int3(end.x, end.y, end.z);
                    job.voxels = chunk.voxels;
                    job.block = index;

                    jobs[jobIndex] = job.Schedule();
                    jobIndex++;
                }
            }
        }

        // required for agent navigation
        EditVoxelJob navJob = new EditVoxelJob()
        {
            size = new int3(_world.size.x, _world.size.y, _world.size.z),
            start = new int3(start.x, start.y, start.z),
            end = new int3(end.x, end.y, end.z),
            voxels = _world.data.voxels,
            block = index
        };
        JobHandle navHandle = navJob.Schedule();

        // combine dependencies and refresh the chunks
        editHandle = JobHandle.CombineDependencies(jobs);
        editHandle = JobHandle.CombineDependencies(editHandle, navHandle);
        foreach (var item in m_editChunks) { item.Refresh(editHandle); }

        m_editChunks.Clear();
        jobs.Dispose();

        // notify listeners
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
