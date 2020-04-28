using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct AvoidCollisionBehavior : IJobParallelFor
    {
        public float maxSpeed;
        public float maxAvoidForce;
        public float maxAvoidRadius;

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
        public NativeMultiHashMap<int3, float3> spatialMap;

        public void Execute(int i)
        {
            float dynamicLength = math.length(velocity[i]) / maxSpeed;
            float3 direction = math.normalizesafe(velocity[i], float3.zero);
            float3 closest = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

            float3 ahead  = position[i] + direction * dynamicLength;
            float3 ahead2 = position[i] + direction * dynamicLength * 0.5f;

            float3 min = ahead + new float3(-maxAvoidRadius, -maxAvoidRadius, -maxAvoidRadius);
            float3 max = ahead + new float3( maxAvoidRadius,  maxAvoidRadius,  maxAvoidRadius);
            float3 min2 = ahead2 + new float3(-maxAvoidRadius, -maxAvoidRadius, -maxAvoidRadius);
            float3 max2 = ahead2 + new float3( maxAvoidRadius,  maxAvoidRadius,  maxAvoidRadius);

            int3 minBucket = math.min(GetSpatialBucket(min), GetSpatialBucket(min2));
            int3 maxBucket = math.max(GetSpatialBucket(max), GetSpatialBucket(max2));

            bool shouldAvoid;
            if (minBucket.Equals(maxBucket))
                shouldAvoid = DetectObstruction(i, ahead, ahead2, minBucket, ref closest);
            else
                shouldAvoid = DetectObstruction(i, ahead, ahead2, minBucket, maxBucket, ref closest);

            if (shouldAvoid)
                ApplyAvoidanceForce(i, ahead, closest);
        }

        public bool IntersectsCircle(float3 v1, float3 v2, float3 center, float radius)
        {
            return math.length(v1 - center) <= radius || math.length(v2 - center) <= radius;
        }

        public bool MostThreatening(int i, float3 target, float3 mostThreatening)
        {
            return math.length(target - position[i]) < math.length(mostThreatening - position[i]);
        }

        public void ApplyAvoidanceForce(int i, float3 ahead, float3 center)
        {
            float3 avoidance = ahead - center;
            avoidance = math.normalizesafe(avoidance, float3.zero);
            avoidance *= maxAvoidForce;
            steering[i] += avoidance;
        }

        public bool DetectObstruction(int i, float3 ahead, float3 ahead2, int3 min, int3 max, ref float3 closest)
        {
            bool obstructed = false;
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
                        if (DetectObstruction(i, ahead, ahead2, bucket, ref closest))
                            obstructed = true;
                    }
                }
            }

            return obstructed;
        }

        public bool DetectObstruction(int i, float3 ahead, float3 ahead2, int3 bucket, ref float3 closest)
        {
            bool hasValues;
            bool obstructed = false;
            float3 agent = float3.zero;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            int maxDepth = 32;

            hasValues = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValues)
            {
                if (count == maxDepth) { break; }
                count++;

                if (!position[i].Equals(agent) && IntersectsCircle(ahead, ahead2, agent, maxAvoidRadius))
                {
                    if (MostThreatening(i, agent, closest)) { closest = agent; }
                    obstructed = true;
                }

                hasValues = spatialMap.TryGetNextValue(out agent, ref iter);
            }

            return obstructed;
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
