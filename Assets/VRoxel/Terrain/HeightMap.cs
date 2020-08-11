using UnityEngine;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

using VRoxel.Terrain.HeightMaps;

namespace VRoxel.Terrain
{
    /// <summary>
    /// A 2D height map of voxel terrain
    /// </summary>
    public class HeightMap : MonoBehaviour
    {
        /// <summary>
        /// The total (x,z) size of the height map
        /// plus the maximum height (y) value
        /// </summary>
        public Vector3Int size;

        /// <summary>
        /// A reference to the 3D array of voxel terrain
        /// </summary>
        public NativeArray<byte> voxels;

        /// <summary>
        /// The cached 2D array of terrain height values
        /// </summary>
        NativeArray<ushort> m_data;

        /// <summary>
        /// Handle for the background job to refresh the height map
        /// </summary>
        JobHandle m_refreshing;

        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        #endregion
        //-------------------------------------------------

        //-------------------------------------------------
        #region Public API

        /// <summary>
        /// Initializes the array of height map values
        /// </summary>
        public void Initialize()
        {
            m_data = new NativeArray<ushort>(
                size.x * size.z, Allocator.Persistent
            );
        }

        /// <summary>
        /// Schedules a background job to update the height map
        /// </summary>
        public JobHandle Refresh(JobHandle dependsOn = default(JobHandle))
        {
            if (!m_refreshing.IsCompleted) { m_refreshing.Complete(); }

            int batch = 1;
            int length = size.x * size.z;
            UpdateHeightMap job = new UpdateHeightMap()
            {
                size = new int3(size.x, size.y, size.z),
                voxels = voxels,
                data = m_data,
            };

            m_refreshing = job.Schedule(length, batch, dependsOn);
            return m_refreshing;
        }

        /// <summary>
        /// Returns the height of the terrain at (x,z)
        /// </summary>
        public ushort Read(int x, int z)
        {
            if (!Contains(x,z))
                return ushort.MaxValue;

            /// 2D[x,y] = 2D[x * height + y]
            return m_data[x * size.z + z];
        }

        /// <summary>
        /// Checks if (x,z) is inside the height map
        /// </summary>
        public bool Contains(int x, int z)
        {
            if (x < 0 || x >= size.x) { return false; }
            if (z < 0 || z >= size.z) { return false; }
            return true;
        }

        /// <summary>
        /// Disposes all unmanaged memory
        /// </summary>
        public void Dispose()
        {
            m_data.Dispose();
        }

        #endregion
        //-------------------------------------------------
    }
}
