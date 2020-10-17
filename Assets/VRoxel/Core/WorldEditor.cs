using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

using VRoxel.Core.Chunks;

namespace VRoxel.Core
{
    /// <summary>
    /// A collection of functions to help with editing a voxel world
    /// </summary>
    public class WorldEditor
    {
        /// <summary>
        /// Set the block adjacent to the hit position in the voxel world.
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The new block index to set</param>
        public static void AddBlock(World world, RaycastHit hit, byte block)
        {
            Vector3 position = world.AdjustRaycastHit(hit, Cube.Point.Outside);
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Update the block at the hit position in the voxel world
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="hit">The RaycastHit to be adjusted</param>
        /// <param name="block">The new block index to set</param>
        public static void ReplaceBlock(World world, RaycastHit hit, byte block)
        {
            Vector3 position = world.AdjustRaycastHit(hit, Cube.Point.Inside);
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Updates a single block in the voxel world and flags the chunk as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="position">A position in the scene</param>
        /// <param name="block">The new block index to set</param>
        public static void SetBlock(World world, Vector3 position, byte block)
        {
            Vector3Int point = world.SceneToGrid(position);
            world.Write(point, block);
        }

        /// <summary>
        /// Updates blocks in the voxel world using Moore neighborhoods and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="position">The center of the neighborhood</param>
        /// <param name="range">The Moore neighborhood size</param>
        /// <param name="block">The new block index to set</param>
        public static void SetNeighborhood(World world, Vector3 position, int range, byte block)
        {
            int neighborhood = 2 * range + 1;
            Vector3Int offset = Vector3Int.zero;
            Vector3Int point = world.SceneToGrid(position);

            for (int i = 0; i < neighborhood; i++) // x-axis
            {
                offset.x = point.x + i - range;
                for (int j = 0; j < neighborhood; j++) // z-axis
                {
                    offset.z = point.z + j - range;
                    for (int k = 0; k < neighborhood; k++) // y-axis
                    {
                        offset.y = point.y + k - range;
                        world.Write(offset, block);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a rectangle of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="start">The start position of the rectangle in the scene</param>
        /// <param name="end">The end position of the rectangle in the scene</param>
        /// <param name="block">The new block index to set</param>
        /// <param name="handle">The job handle(s) for updating the world</param>
        public static void SetRectangle(World world, Vector3 start, Vector3 end, byte block, ref JobHandle handle)
        {
            Vector3Int endGrid = world.SceneToGrid(end);
            Vector3Int startGrid = world.SceneToGrid(start);
            SetRectangle(world, startGrid, endGrid, block, ref handle);
        }

        /// <summary>
        /// Updates a rectangle of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the voxel world</param>
        /// <param name="start">The starting global position in the voxel world</param>
        /// <param name="end">The end global position in the voxel world</param>
        /// <param name="block">The new block index to set</param>
        /// <param name="handle">The job handle(s) for updating the world</param>
        public static void SetRectangle(World world, Vector3Int start, Vector3Int end, byte block, ref JobHandle handle)
        {
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
            Vector3Int chunkEnd   = world.chunkManager.IndexFrom(end);
            Vector3Int chunkStart = world.chunkManager.IndexFrom(start);
            Vector3Int chunkSize = world.chunkManager.configuration.size;
            Vector3Int chunkMax = new Vector3Int(
                world.size.x / chunkSize.x,
                world.size.y / chunkSize.y,
                world.size.z / chunkSize.z
            );

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
                        chunk = world.chunkManager.Get(chunkIndex);
                        if (chunk == null) { continue; }

                        chunk.buildingMesh.Complete();
                        if (chunk.neighbors.up)    { chunk.neighbors.up.buildingMesh.Complete(); }
                        if (chunk.neighbors.down)  { chunk.neighbors.down.buildingMesh.Complete(); }
                        if (chunk.neighbors.north) { chunk.neighbors.north.buildingMesh.Complete(); }
                        if (chunk.neighbors.south) { chunk.neighbors.south.buildingMesh.Complete(); }
                        if (chunk.neighbors.east)  { chunk.neighbors.east.buildingMesh.Complete(); }
                        if (chunk.neighbors.west)  { chunk.neighbors.west.buildingMesh.Complete(); }

                        world.chunkManager.Refresh(chunkIndex);
                        // update neighboring chunks
                        //
                        // check if the rectangles minimum x is a local minimum for the chunk
                        // and the chunk is not the first chunk on the X axis
                        if (rectMin.x - (chunkIndex.x * chunkSize.x) == 0 && chunkIndex.x != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.West);

                        // check if the rectangles maximum x is a local maximum for the chunk
                        // and the chunk is not the last chunk on the X axis
                        if (rectMin.x + rectDelta.x - (chunkIndex.x * chunkSize.x) == chunkSize.x && chunkIndex.x != chunkMax.x - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.East);

                        // check if the rectangles minimum y is a local minimum for the chunk
                        // and the chunk is not the first chunk on the Y axis
                        if (rectMin.y - (chunkIndex.y * chunkSize.y) == 0 && chunkIndex.y != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.Down);

                        // check if the rectangles maximum y is a local maximum for the chunk
                        // and the chunk is not the last chunk on the Y axis
                        if (rectMin.y + rectDelta.y - (chunkIndex.y * chunkSize.y) == chunkSize.y && chunkIndex.y != chunkMax.y - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.Up);

                        // check if the rectangles minimum z is a local minimum for the chunk
                        // and the chunk is not the first chunk on the Z axis
                        if (rectMin.z - (chunkIndex.z * chunkSize.z) == 0 && chunkIndex.z != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.South);

                        // check if the rectangles maximum z is a local maximum for the chunk
                        // and the chunk is not the last chunk on the Z axis
                        if (rectMin.z + rectDelta.z - (chunkIndex.z * chunkSize.z) == chunkSize.z && chunkIndex.z != chunkMax.z - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.North);

                        // schedule a background job to update the chunks voxel data
                        ModifyRectangle job = new ModifyRectangle();
                        job.blockLibrary = world.chunkManager.meshGenerator.blockLibrary;
                        job.chunkOffset = new int3(chunk.offset.x, chunk.offset.y, chunk.offset.z);
                        job.chunkSize = new int3(chunkSize.x, chunkSize.y, chunkSize.z);
                        job.start = new int3(start.x, start.y, start.z);
                        job.end = new int3(end.x, end.y, end.z);
                        job.voxels = chunk.voxels;
                        job.block = block;

                        jobs[jobIndex] = job.Schedule();
                        jobIndex++;
                    }
                }
            }

            // required for agent navigation
            EditVoxelJob navJob = new EditVoxelJob()
            {
                size = new int3(world.size.x, world.size.y, world.size.z),
                start = new int3(start.x, start.y, start.z),
                end = new int3(end.x, end.y, end.z),
                voxels = world.data.voxels,
                block = block
            };
            JobHandle navHandle = navJob.Schedule();

            // combine dependencies
            handle = JobHandle.CombineDependencies(jobs);
            handle = JobHandle.CombineDependencies(handle, navHandle);
            world.chunkManager.refreshDependsOn = handle;
            jobs.Dispose();
        }

        /// <summary>
        /// Updates a sphere of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="block">The new block index to set</param>
        public static void SetSphere(World world, Vector3 position, float radius, byte block, ref JobHandle handle)
        {
            Vector3Int center = world.SceneToGrid(position);
            SetSphere(world, center, radius, block, ref handle);
        }

        /// <summary>
        /// Updates a sphere of blocks in the voxel world and flags the affected chunks as modified
        /// </summary>
        /// <param name="world">A reference to the World</param>
        /// <param name="position">The center of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="block">The new block index to set</param>
        public static void SetSphere(World world, Vector3Int position, float radius, byte block, ref JobHandle handle)
        {
            // calculate min and delta of a rectangle that encloses the sphere
            int size = Mathf.CeilToInt(radius);
            Vector3Int rectDelta = new Vector3Int();
            Vector3Int rectMin = new Vector3Int();

            rectDelta.x = size * 2;
            rectDelta.y = size * 2;
            rectDelta.z = size * 2;

            rectMin.x = position.x - size;
            rectMin.y = position.y - size;
            rectMin.z = position.z - size;

            // find the chunks that the rectangle intersects
            // and schedule jobs to update their voxel data

            Vector3Int chunkMin = Vector3Int.zero;
            Vector3Int chunkDelta = Vector3Int.zero;
            Vector3Int chunkStart = world.chunkManager.IndexFrom(rectMin);
            Vector3Int chunkEnd   = world.chunkManager.IndexFrom(rectMin + rectDelta);
            Vector3Int chunkSize = world.chunkManager.configuration.size;
            Vector3Int chunkMax = new Vector3Int(
                world.size.x / chunkSize.x,
                world.size.y / chunkSize.y,
                world.size.z / chunkSize.z
            );

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
                        chunk = world.chunkManager.Get(chunkIndex);
                        if (chunk == null) { continue; }

                        chunk.buildingMesh.Complete();
                        if (chunk.neighbors.up)    { chunk.neighbors.up.buildingMesh.Complete(); }
                        if (chunk.neighbors.down)  { chunk.neighbors.down.buildingMesh.Complete(); }
                        if (chunk.neighbors.north) { chunk.neighbors.north.buildingMesh.Complete(); }
                        if (chunk.neighbors.south) { chunk.neighbors.south.buildingMesh.Complete(); }
                        if (chunk.neighbors.east)  { chunk.neighbors.east.buildingMesh.Complete(); }
                        if (chunk.neighbors.west)  { chunk.neighbors.west.buildingMesh.Complete(); }

                        world.chunkManager.Refresh(chunkIndex);
                        // update neighboring chunks
                        //
                        // check if the rectangles minimum x is a local minimum for the chunk
                        // and the chunk is not the first chunk on the X axis
                        if (rectMin.x - (chunkIndex.x * chunkSize.x) == 0 && chunkIndex.x != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.West);

                        // check if the rectangles maximum x is a local maximum for the chunk
                        // and the chunk is not the last chunk on the X axis
                        if (rectMin.x + rectDelta.x - (chunkIndex.x * chunkSize.x) == chunkSize.x && chunkIndex.x != chunkMax.x - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.East);

                        // check if the rectangles minimum y is a local minimum for the chunk
                        // and the chunk is not the first chunk on the Y axis
                        if (rectMin.y - (chunkIndex.y * chunkSize.y) == 0 && chunkIndex.y != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.Down);

                        // check if the rectangles maximum y is a local maximum for the chunk
                        // and the chunk is not the last chunk on the Y axis
                        if (rectMin.y + rectDelta.y - (chunkIndex.y * chunkSize.y) == chunkSize.y && chunkIndex.y != chunkMax.y - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.Up);

                        // check if the rectangles minimum z is a local minimum for the chunk
                        // and the chunk is not the first chunk on the Z axis
                        if (rectMin.z - (chunkIndex.z * chunkSize.z) == 0 && chunkIndex.z != 0)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.South);

                        // check if the rectangles maximum z is a local maximum for the chunk
                        // and the chunk is not the last chunk on the Z axis
                        if (rectMin.z + rectDelta.z - (chunkIndex.z * chunkSize.z) == chunkSize.z && chunkIndex.z != chunkMax.z - 1)
                            world.chunkManager.Refresh(chunkIndex + Direction3Int.North);

                        // schedule a background job to update the chunks voxel data

                        ModifySphere job = new ModifySphere();
                        job.blockLibrary = world.chunkManager.meshGenerator.blockLibrary;
                        job.chunkOffset = new int3(chunk.offset.x, chunk.offset.y, chunk.offset.z);
                        job.chunkSize = new int3(chunkSize.x, chunkSize.y, chunkSize.z);
                        job.center = new int3(position.x, position.y, position.z);
                        job.voxels = chunk.voxels;
                        job.radius = radius;
                        job.block = block;

                        jobs[jobIndex] = job.Schedule();
                        jobIndex++;
                    }
                }
            }

            // deprecated but still required for agent navigation
            EditVoxelSphereJob navJob = new EditVoxelSphereJob()
            {
                worldSize = new int3(world.size.x, world.size.y, world.size.z),
                position = new int3(position.x, position.y, position.z),
                voxels = world.data.voxels,
                radius = radius,
                block = block
            };
            JobHandle navHandle = navJob.Schedule();

            // combine dependencies and refresh the chunks
            handle = JobHandle.CombineDependencies(jobs);
            handle = JobHandle.CombineDependencies(handle, navHandle);
            world.chunkManager.refreshDependsOn = handle;
            jobs.Dispose();
        }
    }
}
