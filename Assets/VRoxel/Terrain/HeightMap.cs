using UnityEngine;
using VRoxel.Core;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace VRoxel.Terrain
{
    /// <summary>
    /// Represents a 2D array of height values for voxel terrain
    /// </summary>
    public class HeightMap : MonoBehaviour
    {
        /// <summary>
        /// The reference to the voxel world
        /// </summary>
        [Tooltip("Can be left blank if the object has a World component")]
        public World world;

        /// <summary>
        /// The maximum height value the height map can have
        /// </summary>
        public ushort maxHeight;

        /// <summary>
        /// The total (x,y) size of the height map
        /// </summary>
        public Vector2Int size;

        /// <summary>
        /// A 2D array of height values for the terrain
        /// </summary>
        NativeArray<ushort> m_data;

        //-------------------------------------------------
        #region Monobehaviors

        protected virtual void Awake()
        {
            if (world == null)
                world = GetComponent<World>();
        }

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
                size.x * size.y, Allocator.Persistent
            );
        }

        /// <summary>
        /// Returns the height of the terrain at the point (x,y)
        /// </summary>
        public ushort Read(int x, int y)
        {
            if (!Contains(x,y))
                return ushort.MaxValue;

            return m_data[Flatten(x,y)];
        }

        /// <summary>
        /// Converts a (x,y) position into an array index
        /// </summary>
        public int Flatten(int x, int y)
        {
            /// A[x,y] = A[x * height + y]
            return x * size.y + y;
        }

        /// <summary>
        /// Checks if a (x,y) position is inside the height map
        /// </summary>
        public bool Contains(int x, int y)
        {
            if (x < 0 || x >= size.x) { return false; }
            if (y < 0 || y >= size.y) { return false; }
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
