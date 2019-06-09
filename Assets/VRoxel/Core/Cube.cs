using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static reference for a Voxel cube
/// </summary>
public static class Cube
{
    /// <summary>
    /// the cardinal directions for a cube
    /// </summary>
    public enum Direction
    {
        Top, Bottom, North, East, South, West
    }

    /// <summary>
    /// unit vectors for a cube with an origin at (0,0,0)
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
    public static Vector3[] Vectors = {
        new Vector3(-1,  1,  1), // 0
        new Vector3( 1,  1,  1), // 1
        new Vector3( 1,  1, -1), // 2
        new Vector3(-1,  1, -1), // 3
        new Vector3(-1, -1,  1), // 4
        new Vector3( 1, -1,  1), // 5
        new Vector3( 1, -1, -1), // 6
        new Vector3(-1, -1, -1)  // 7
    };

    /// <summary>
    /// the unit vectors for each face of a cube
    /// </summary>
    //
    // Note: Use the Cube.Direction enum to access the first array dimension
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
    public static int[][] Faces = {
        new int[] { 0, 1, 2, 3 }, // Top
        new int[] { 7, 6, 5, 4 }, // Bottom
        new int[] { 1, 0, 4, 5 }, // North
        new int[] { 2, 1, 5, 6 }, // East
        new int[] { 3, 2, 6, 7 }, // South
        new int[] { 0, 3, 7, 4 }  // West
    };

    /// <summary>
    /// Calculates the face vertices for a Cube
    /// </summary>
    public static void Face(int direction, Vector3 position, float scale, ref Vector3[] face)
    {
        for (int i = 0; i < 4; i++)
        {
            face[i] = Vectors[Faces[direction][i]];
            face[i] *= scale;
            face[i] += position;
        }
    }

    /// <summary>
    /// Calculates the transform for a Cube
    /// </summary>
    public static void Transform(Vector3 position, float scale, Quaternion rotation, ref Vector3[] cube)
    {
        for (int i = 0; i < 8; i++)
        {
            cube[i] = Vectors[i];
            cube[i] *= scale;
            cube[i] = rotation * cube[i];
            cube[i] += position;
        }
    }

    /// <summary>
    /// Calculates the transform for a rectangle
    /// </summary>
    public static void TransformRectangle(Vector3 start, Vector3 end, Vector3 scale, Quaternion rotation, ref Vector3[] rectangle)
    {
        Vector3 center = Vector3.Lerp(start, end, 0.5f);
        for (int i = 0; i < 8; i++)
        {
            rectangle[i] = Vectors[i];
            rectangle[i].x *= scale.x;
            rectangle[i].y *= scale.y;
            rectangle[i].z *= scale.z;
            rectangle[i] = rotation * rectangle[i];
            rectangle[i] += center;
        }
    }
}
