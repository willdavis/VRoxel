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

    /// <summary>
    /// The maximum number of chunks that can be refreshed each frame
    /// </summary>
    [Min(1)]
    [Tooltip("Limit the number of chunks that can be refreshed each frame")]
    public int refreshPerFrame = 1;


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
    Queue<Chunk> m_refreshChunks;

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
        m_refreshChunks = new Queue<Chunk>();
    }

    void Update()
    {
        int refreshCount = refreshPerFrame;
        while (m_refreshChunks.Count != 0 && refreshCount > 0)
        {
            m_refreshChunks.Dequeue().Refresh(editHandle);
            refreshCount--;
        }

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

        // calculate min and delta of the rectangle so the
        // orientation of the start and end positions will not matter

        Vector3Int rectDelta = new Vector3Int();
        Vector3Int rectMin = new Vector3Int();

        rectDelta.x = Mathf.Abs(end.x - start.x) + 1;
        rectDelta.y = Mathf.Abs(end.y - start.y) + 1;
        rectDelta.z = Mathf.Abs(end.z - start.z) + 1;

        rectMin.x = Mathf.Min(start.x, end.x);
        rectMin.y = Mathf.Min(start.y, end.y);
        rectMin.z = Mathf.Min(start.z, end.z);

        // find the chunks that the rectangle intersects
        // and schedule jobs to update their voxel data

        Vector3Int chunkMin = Vector3Int.zero;
        Vector3Int chunkDelta = Vector3Int.zero;
        Vector3Int chunkEnd   = _world.chunkManager.IndexFrom(end);
        Vector3Int chunkStart = _world.chunkManager.IndexFrom(start);
        Vector3Int chunkSize = _world.chunkManager.configuration.size;
        Vector3Int chunkMax = new Vector3Int(
            _world.size.x / chunkSize.x,
            _world.size.y / chunkSize.y,
            _world.size.z / chunkSize.z
        );

        chunkDelta.x = Mathf.Abs(chunkEnd.x - chunkStart.x) + 1;
        chunkDelta.y = Mathf.Abs(chunkEnd.y - chunkStart.y) + 1;
        chunkDelta.z = Mathf.Abs(chunkEnd.z - chunkStart.z) + 1;

        chunkMin.x = Mathf.Min(chunkStart.x, chunkEnd.x);
        chunkMin.y = Mathf.Min(chunkStart.y, chunkEnd.y);
        chunkMin.z = Mathf.Min(chunkStart.z, chunkEnd.z);

        Chunk chunk;
        Chunk nextChunk;
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
                    chunk = _world.chunkManager.Get(chunkIndex);
                    m_refreshChunks.Enqueue(chunk);

                    // update neighboring chunks
                    //
                    // check if the rectangles minimum x is a local minimum for the chunk
                    // and the chunk is not the first chunk on the X axis
                    if (rectMin.x - (chunkIndex.x * chunkSize.x) == 0 && chunkIndex.x != 0)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.West);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // check if the rectangles maximum x is a local maximum for the chunk
                    // and the chunk is not the last chunk on the X axis
                    if (rectMin.x + rectDelta.x - (chunkIndex.x * chunkSize.x) == chunkSize.x && chunkIndex.x != chunkMax.x - 1)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.East);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // check if the rectangles minimum y is a local minimum for the chunk
                    // and the chunk is not the first chunk on the Y axis
                    if (rectMin.y - (chunkIndex.y * chunkSize.y) == 0 && chunkIndex.y != 0)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.Down);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // check if the rectangles maximum y is a local maximum for the chunk
                    // and the chunk is not the last chunk on the Y axis
                    if (rectMin.y + rectDelta.y - (chunkIndex.y * chunkSize.y) == chunkSize.y && chunkIndex.y != chunkMax.y - 1)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.Up);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // check if the rectangles minimum z is a local minimum for the chunk
                    // and the chunk is not the first chunk on the Z axis
                    if (rectMin.z - (chunkIndex.z * chunkSize.z) == 0 && chunkIndex.z != 0)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.South);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // check if the rectangles maximum z is a local maximum for the chunk
                    // and the chunk is not the last chunk on the Z axis
                    if (rectMin.z + rectDelta.z - (chunkIndex.z * chunkSize.z) == chunkSize.z && chunkIndex.z != chunkMax.z - 1)
                    {
                        nextChunk = _world.chunkManager.Get(chunkIndex + Direction3Int.North);
                        if (nextChunk) { m_refreshChunks.Enqueue(nextChunk); }
                    }

                    // schedule a background job to update the chunks voxel data

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
