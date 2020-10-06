using Unity.Mathematics;
using UnityEngine;

namespace VRoxel.Core
{
    /// <summary>
    /// Static references for a Voxel cube
    /// </summary>
    public static class Cube
    {
        /// <summary>
        /// the 6 cardinal directions around a cube
        /// </summary>
        public enum Direction
        {
            Top, Bottom, North, East, South, West
        }

        /// <summary>
        /// the unit vectors for each cardinal direction around a cube
        /// </summary>
        public static int3[] Directions = {
            new int3( 0,  1,  0), // Top
            new int3( 0, -1,  0), // Bottom
            new int3( 0,  0,  1), // North (Front)
            new int3( 1,  0,  0), // East  (Right)
            new int3( 0,  0, -1), // South (Back)
            new int3(-1,  0,  0), // West  (Left)
        };

        /// <summary>
        /// the unit vectors for each cardinal direction around a cube
        /// </summary>
        public static Vector3Int[] Directions3Int = {
            new Vector3Int( 0,  1,  0), // Top
            new Vector3Int( 0, -1,  0), // Bottom
            new Vector3Int( 0,  0,  1), // North (Front)
            new Vector3Int( 1,  0,  0), // East  (Right)
            new Vector3Int( 0,  0, -1), // South (Back)
            new Vector3Int(-1,  0,  0), // West  (Left)
        };

        /// <summary>
        /// the places a point can be relative to a cube
        /// </summary>
        public enum Point
        {
            Inside, Outside
        }

        /// <summary>
        /// the unit vectors for a cube with an origin at (0,0,0)
        /// </summary>
        //
        //                        Y  Z
        //      0------1          | /
        //     /|     /|          |/
        //    3------2 |   -X ----*---- X
        //    | 4-*--|-5         /|
        //    |/     |/         / |
        //    7------6        -Z -Y
        //
        public static float3[] Vectors = {
            new float3(-1,  1,  1), // 0
            new float3( 1,  1,  1), // 1
            new float3( 1,  1, -1), // 2
            new float3(-1,  1, -1), // 3
            new float3(-1, -1,  1), // 4
            new float3( 1, -1,  1), // 5
            new float3( 1, -1, -1), // 6
            new float3(-1, -1, -1)  // 7
        };

        /// <summary>
        /// references the unit vectors for each of the 6 cube faces
        /// </summary>
        //
        // Note: Indexes are returned in a clockwise order for each face
        //
        //             Top
        //              |   North
        //              |  /
        //              | /
        //              |/
        //   West ------*------ East
        //             /|
        //            / |
        //           /  |
        //      South   |
        //            Bottom
        //
        public static int[] Faces = {
            0, 1, 2, 3, // Top
            7, 6, 5, 4, // Bottom
            1, 0, 4, 5, // North
            2, 1, 5, 6, // East
            3, 2, 6, 7, // South
            0, 3, 7, 4  // West
        };

        /// <summary>
        /// Calculates the face vertices for a Cube
        /// </summary>
        public static void Face(int direction, float3 position, float scale, ref Vector3[] face)
        {
            int index;
            for (int i = 0; i < 4; i++)
            {
                index = direction * 4 + i;
                face[i] = Vectors[Faces[index]];
                face[i] *= scale;
                face[i] += new Vector3(
                    position.x,
                    position.y,
                    position.z
                );
            }
        }

        /// <summary>
        /// Calculates the transform for a Cube
        /// </summary>
        public static void Transform(
            float3 position, float scale,
            quaternion rotation, ref float3[] cube)
        {
            for (int i = 0; i < 8; i++)
            {
                cube[i] = Vectors[i];
                cube[i] *= scale;
                cube[i] = math.rotate(
                    rotation, cube[i]);
                cube[i] += position;
            }
        }

        /// <summary>
        /// Calculates the transform for a Rectangle
        /// </summary>
        public static void TransformRectangle(
            float3 start, float3 end, float3 scale,
            quaternion rotation, ref float3[] rectangle)
        {
            float3 center = math.lerp(start, end, 0.5f);
            for (int i = 0; i < 8; i++)
            {
                rectangle[i] = Vectors[i];
                rectangle[i].x *= scale.x;
                rectangle[i].y *= scale.y;
                rectangle[i].z *= scale.z;
                rectangle[i] = math.rotate(
                    rotation, rectangle[i]);
                rectangle[i] += center;
            }
        }
    }
}
