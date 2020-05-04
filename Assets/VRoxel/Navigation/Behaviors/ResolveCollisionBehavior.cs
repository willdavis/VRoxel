using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace VRoxel.Navigation
{
    [BurstCompile]
    public struct ResolveCollisionBehavior : IJobParallelFor
    {
        public int maxDepth;
        public float collisionForce;
        public float collisionRadius;

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

            float3 min = position[i] + new float3(-collisionRadius, -collisionRadius, -collisionRadius);
            float3 max = position[i] + new float3( collisionRadius,  collisionRadius,  collisionRadius);

            int3 minBucket = GetSpatialBucket(min);
            int3 maxBucket = GetSpatialBucket(max);

            if (minBucket.Equals(maxBucket))
                ResolveCollisions(i, minBucket);
            else
                ResolveCollisions(i, minBucket, maxBucket);
        }

        public bool Collision(float3 self, float3 target, float radius)
        {
            return math.length(self - target) <= radius;
        }

        public void ApplyCollisionForce(int i, float3 center)
        {
            float3 collision = position[i] - center;
            float length = math.length(position[i] - center);
            float scale = (collisionRadius / length) * collisionForce;

            collision = math.normalizesafe(collision, float3.zero);

            steering[i] += collision * scale;
            steering[i] += -velocity[i] * scale;
        }

        public void ResolveCollisions(int i, int3 min, int3 max)
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
                        ResolveCollisions(i, bucket);
                    }
                }
            }
        }

        public void ResolveCollisions(int i, int3 bucket)
        {
            bool hasValue;
            float3 agent = float3.zero;
            NativeMultiHashMapIterator<int3> iter;

            int count = 0;
            hasValue = spatialMap.TryGetFirstValue(bucket, out agent, out iter);
            while (hasValue)
            {
                if (count == maxDepth) { break; }
                count++;

                if (!position[i].Equals(agent) && Collision(position[i], agent, collisionRadius))
                    ApplyCollisionForce(i, agent);

                hasValue = spatialMap.TryGetNextValue(out agent, ref iter);
            }
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
