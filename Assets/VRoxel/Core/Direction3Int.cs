using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// Contains Vector3Int unit vectors for a total of 27 directions
    /// </summary>
    public static class Direction3Int
    {
        public enum Name
        {
            Zero, Up, Down,
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest,
            UpNorth, UpNorthEast, UpEast, UpSouthEast, UpSouth, UpSouthWest, UpWest, UpNorthWest,
            DownNorth, DownNorthEast, DownEast, DownSouthEast, DownSouth, DownSouthWest, DownWest, DownNorthWest
        }

        public static Vector3Int[] Directions = {
            new Vector3Int( 0, 0, 0),   // Zero
            new Vector3Int( 0, 1, 0),   // Up
            new Vector3Int( 0,-1, 0),   // Down
            new Vector3Int( 0, 0, 1),   // North
            new Vector3Int( 1, 0, 1),   // NorthEast
            new Vector3Int( 1, 0, 0),   // East
            new Vector3Int( 1, 0,-1),   // SouthEast
            new Vector3Int( 0, 0,-1),   // South
            new Vector3Int(-1, 0,-1),   // SouthWest
            new Vector3Int(-1, 0, 0),   // West
            new Vector3Int(-1, 0, 1),   // NorthWest
            new Vector3Int( 0, 1, 1),   // UpNorth
            new Vector3Int( 1, 1, 1),   // UpNorthEast
            new Vector3Int( 1, 1, 0),   // UpEast
            new Vector3Int( 1, 1,-1),   // UpSouthEast
            new Vector3Int( 0, 1,-1),   // UpSouth
            new Vector3Int(-1, 1,-1),   // UpSouthWest
            new Vector3Int(-1, 1, 0),   // UpWest
            new Vector3Int(-1, 1, 1),   // UpNorthWest
            new Vector3Int( 0,-1, 1),   // DownNorth
            new Vector3Int( 1,-1, 1),   // DownNorthEast
            new Vector3Int( 1,-1, 0),   // DownEast
            new Vector3Int( 1,-1,-1),   // DownSouthEast
            new Vector3Int( 0,-1,-1),   // DownSouth
            new Vector3Int(-1,-1,-1),   // DownSouthWest
            new Vector3Int(-1,-1, 0),   // DownWest
            new Vector3Int(-1,-1, 1),   // DownNorthWest
        };

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 0, 0)
        /// </summary>
        public static Vector3Int Zero = Directions[(int)Name.Zero];


        #region Standard Compass Directions

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 0, 1)
        /// </summary>
        public static Vector3Int North = Directions[(int)Name.North];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, 1)
        /// </summary>
        public static Vector3Int NorthEast = Directions[(int)Name.NorthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, 0)
        /// </summary>
        public static Vector3Int East = Directions[(int)Name.East];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 0, -1)
        /// </summary>
        public static Vector3Int SouthEast = Directions[(int)Name.SouthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 0, -1)
        /// </summary>
        public static Vector3Int South = Directions[(int)Name.South];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, -1)
        /// </summary>
        public static Vector3Int SouthWest = Directions[(int)Name.SouthWest];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, 0)
        /// </summary>
        public static Vector3Int West = Directions[(int)Name.West];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 0, 1)
        /// </summary>
        public static Vector3Int NorthWest = Directions[(int)Name.NorthWest];
        #endregion

        #region Top Compass Directions
        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, 0)
        /// </summary>
        public static Vector3Int Up = Directions[(int)Name.Up];

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, 1)
        /// </summary>
        public static Vector3Int UpNorth = Directions[(int)Name.UpNorth];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, 1)
        /// </summary>
        public static Vector3Int UpNorthEast = Directions[(int)Name.UpNorthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, 0)
        /// </summary>
        public static Vector3Int UpEast = Directions[(int)Name.UpEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, 1, -1)
        /// </summary>
        public static Vector3Int UpSouthEast = Directions[(int)Name.UpSouthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, 1, -1)
        /// </summary>
        public static Vector3Int UpSouth = Directions[(int)Name.UpSouth];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, -1)
        /// </summary>
        public static Vector3Int UpSouthWest = Directions[(int)Name.UpSouthWest];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, 0)
        /// </summary>
        public static Vector3Int UpWest = Directions[(int)Name.UpWest];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, 1, 1)
        /// </summary>
        public static Vector3Int UpNorthWest = Directions[(int)Name.UpNorthWest];
        #endregion

        #region Bottom Compass Directions
        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, 0)
        /// </summary>
        public static Vector3Int Down = Directions[(int)Name.Down];

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, 1)
        /// </summary>
        public static Vector3Int DownNorth = Directions[(int)Name.DownNorth];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, 1)
        /// </summary>
        public static Vector3Int DownNorthEast = Directions[(int)Name.DownNorthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, 0)
        /// </summary>
        public static Vector3Int DownEast = Directions[(int)Name.DownEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 1, -1, -1)
        /// </summary>
        public static Vector3Int DownSouthEast = Directions[(int)Name.DownSouthEast];

        /// <summary>
        /// Shorthand for writing Vector3Int( 0, -1, -1)
        /// </summary>
        public static Vector3Int DownSouth = Directions[(int)Name.DownSouth];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, -1)
        /// </summary>
        public static Vector3Int DownSouthWest = Directions[(int)Name.DownSouthWest];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, 0)
        /// </summary>
        public static Vector3Int DownWest = Directions[(int)Name.DownWest];

        /// <summary>
        /// Shorthand for writing Vector3Int( -1, -1, 1)
        /// </summary>
        public static Vector3Int DownNorthWest = Directions[(int)Name.DownNorthWest];
        #endregion
    }
}
