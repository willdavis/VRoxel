using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct QueueBehavior : IJobParallelFor
    {
        public float maxQueueAhead;
        public float maxQueueRadius;
        public float maxBrakeForce;

        /// <summary>
        /// the size of all spatial buckets
        /// </summary>
        public int3 size;

        /// <summary>
        /// the scale of the voxel world
        /// </summary>
        public float world_scale;

        /// <summary>
        /// the scene offset of the world
        /// </summary>
        public float3 world_offset;

        /// <summary>
        /// the center point of the world
        /// </summary>
        public float3 world_center;

        /// <summary>
        /// the orientation of the world
        /// </summary>
        public quaternion world_rotation;


        public NativeArray<float3> steering;

        [ReadOnly]
        public NativeArray<float3> position;

        [ReadOnly]
        public NativeArray<float3> velocity;

        [ReadOnly]
        public NativeArray<bool> active;

        [ReadOnly]
        public NativeMultiHashMap<int3, float3> spatialMap;

        public void Execute(int i)
        {
            if (!active[i]) { return; }

            float3 direction = math.normalizesafe(velocity[i], float3.zero);
            float3 ahead = position[i] + direction * maxQueueAhead;

            float3 min = ahead + new float3(-maxQueueRadius, -maxQueueRadius, -maxQueueRadius);
            float3 max = ahead + new float3( maxQueueRadius,  maxQueueRadius,  maxQueueRadius);

            int3 minBucket = GetSpatialBucket(min);
            int3 maxBucket = GetSpatialBucket(max);

            bool obstructed;
            if (minBucket.Equals(maxBucket))
                obstructed = DetectCollision(i, ahead, minBucket);
            else
                obstructed = DetectCollision(i, ahead, minBucket, maxBucket);

            if (obstructed)
                ApplyBrakeForce(i);
        }

        public void ApplyBrakeForce(int i)
        {
            steering[i] += -steering[i] * maxBrakeForce;
            steering[i] += -velocity[i];
        }

        public bool DetectCollision(int i, float3 ahead, int3 min, int3 max)
        {
            int3 bucket = int3.zero;
            for (int x = min.x; x < max.x; x++)
            {
                bucket.x = x;
                for (int y = min.y; y < max.y; y++)
                {
                    bucket.y = y;
                    for (int z = min.z; z < max.z; z++)
                    {
                        bucket.z = z;
                        if (DetectCollision(i, ahead, bucket))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool DetectCollision(int i, float3 ahead, int3 bucket)
        {
            bool hasValues;
            float3 agent = float3.zero;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            int maxDepth = 200;

            hasValues = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValues)
            {
                if (count == maxDepth) { break; }
                count++;

                if (!position[i].Equals(agent) && math.length(agent - ahead) <= maxQueueRadius)
                    return true;

                hasValues = spatialMap.TryGetNextValue(out agent, ref iter);
            }

            return false;
        }

        public int3 GetSpatialBucket(float3 position)
        {
            int3 grid = GridPosition(position);
            int3 bucket = new int3(
                grid.x / size.x,
                grid.y / size.y,
                grid.z / size.z
            );
            return bucket;
        }

        /// <summary>
        /// Calculates an int3 (Vector3Int) grid coordinate
        /// from a float3 (Vector3) scene position
        /// </summary>
        /// <param name="position">A position in the scene</param>
        public int3 GridPosition(float3 position)
        {
            float3 adjusted = position;
            int3 gridPosition = int3.zero;
            quaternion rotation = math.inverse(world_rotation);

            adjusted += world_offset * -1f;     // adjust for the worlds offset
            adjusted *= 1 / world_scale;        // adjust for the worlds scale
            adjusted = math.rotate(rotation, adjusted);     // adjust for the worlds rotation
            adjusted += world_center;           // adjust for the worlds center

            gridPosition.x = (int)math.floor(adjusted.x);
            gridPosition.y = (int)math.floor(adjusted.y);
            gridPosition.z = (int)math.floor(adjusted.z);

            return gridPosition;
        }
    }
}