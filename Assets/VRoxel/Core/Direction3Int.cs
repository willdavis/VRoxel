using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// Contains a Vector3Int unit vector for each standard compass direction,
    /// as well as top and bottom compass directions, for a total of 26 directions.
    /// </summary>
    public static class Direction3Int
    {
        #region Standard Compass Directions

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 0, 1)
        /// </summary>
        public static Vector3Int North      = new Vector3Int( 0,  0,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, 1)
        /// </summary>
        public static Vector3Int NorthEast  = new Vector3Int( 1,  0,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, 0)
        /// </summary>
        public static Vector3Int East       = new Vector3Int( 1,  0,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, -1)
        /// </summary>
        public static Vector3Int SouthEast  = new Vector3Int( 1,  0, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 0, -1)
        /// </summary>
        public static Vector3Int South      = new Vector3Int( 0,  0, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, -1)
        /// </summary>
        public static Vector3Int SouthWest  = new Vector3Int(-1,  0, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, 0)
        /// </summary>
        public static Vector3Int West       = new Vector3Int(-1,  0,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, 1)
        /// </summary>
        public static Vector3Int NorthWest  = new Vector3Int(-1,  0,  1);
        #endregion

        #region Top Compass Directions
        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, 0)
        /// </summary>
        public static Vector3Int Up           = new Vector3Int( 0,  1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, 1)
        /// </summary>
        public static Vector3Int UpNorth      = new Vector3Int( 0,  1,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, 1)
        /// </summary>
        public static Vector3Int UpNorthEast  = new Vector3Int( 1,  1,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, 0)
        /// </summary>
        public static Vector3Int UpEast       = new Vector3Int( 1,  1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, -1)
        /// </summary>
        public static Vector3Int UpSouthEast  = new Vector3Int( 1,  1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, -1)
        /// </summary>
        public static Vector3Int UpSouth      = new Vector3Int( 0,  1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, -1)
        /// </summary>
        public static Vector3Int UpSouthWest  = new Vector3Int(-1,  1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, 0)
        /// </summary>
        public static Vector3Int UpWest       = new Vector3Int(-1,  1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, 1)
        /// </summary>
        public static Vector3Int UpNorthWest  = new Vector3Int(-1,  1,  1);
        #endregion

        #region Bottom Compass Directions
        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, 0)
        /// </summary>
        public static Vector3Int Down           = new Vector3Int( 0, -1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, 1)
        /// </summary>
        public static Vector3Int DownNorth      = new Vector3Int( 0, -1,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, 1)
        /// </summary>
        public static Vector3Int DownNorthEast  = new Vector3Int( 1, -1,  1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, 0)
        /// </summary>
        public static Vector3Int DownEast       = new Vector3Int( 1, -1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, -1)
        /// </summary>
        public static Vector3Int DownSouthEast  = new Vector3Int( 1, -1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, -1)
        /// </summary>
        public static Vector3Int DownSouth      = new Vector3Int( 0, -1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, -1)
        /// </summary>
        public static Vector3Int DownSouthWest  = new Vector3Int(-1, -1, -1);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, 0)
        /// </summary>
        public static Vector3Int DownWest       = new Vector3Int(-1, -1,  0);

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, 1)
        /// </summary>
        public static Vector3Int DownNorthWest  = new Vector3Int(-1, -1,  1);
        #endregion
    }
}
