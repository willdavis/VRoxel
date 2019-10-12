using UnityEngine;

/// <summary>
/// Allows the attached GameObject to draw a bounding box around voxels.
/// </summary>
public class BlockCursor : MonoBehaviour
{
    Vector3[] _verts = new Vector3[8];

    /// <summary>
    /// The current block size factor for the cursor.
    /// This is in addition to the world's scale factor.
    /// </summary>
    public float scale = 1f;

    /// <summary>
    /// The LineRenderer attached to this BlockCursor
    /// </summary>
    [HideInInspector]
    public LineRenderer line;

    /// <summary>
    /// Draws a cuboid wire mesh using a LineRenderer.
    /// When p1 == p2 a cube will be drawn, otherwise a rectangle
    /// will be drawn between p1 and p2.
    /// </summary>
    /// <param name="world">The voxel world to use for reference</param>
    /// <param name="p1">The center point of the cube, or the starting point for a rectangle</param>
    /// <param name="p2">The ending point for a rectangle</param>
    public void DrawCuboid(World world, Vector3 p1, Vector3 p2)
    {
        Quaternion rotation = world.transform.rotation;
        float cubeScale = world.scale * scale / 2f;

        if (p1 == p2)
        {
            Cube.Transform(p1, cubeScale, rotation, ref _verts);
        }
        else
        {
            Vector3Int v1 = WorldEditor.Get(world, p1);
            Vector3Int v2 = WorldEditor.Get(world, p2);

            float sizeX = Mathf.Abs(v1.x - v2.x);
            float sizeY = Mathf.Abs(v1.y - v2.y);
            float sizeZ = Mathf.Abs(v1.z - v2.z);

            Vector3 rectangleScale = new Vector3(
                cubeScale * sizeX + cubeScale,
                cubeScale * sizeY + cubeScale,
                cubeScale * sizeZ + cubeScale
            );

            Cube.TransformRectangle(p1, p2, rectangleScale, rotation, ref _verts);
        }

        UpdateLineRenderer();
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void UpdateLineRenderer()
    {
        line.SetPosition(0, _verts[0]);    // set 0
        line.SetPosition(1, _verts[1]);    // set 0 -> 1
        line.SetPosition(2, _verts[2]);    // set 1 -> 2
        line.SetPosition(3, _verts[3]);    // set 2 -> 3
        line.SetPosition(4, _verts[0]);    // set 3 -> 0
        line.SetPosition(5, _verts[4]);    // set 0 -> 4
        line.SetPosition(6, _verts[5]);    // set 4 -> 5
        line.SetPosition(7, _verts[1]);    // set 5 -> 1
        line.SetPosition(8, _verts[2]);    // set 1 -> 2
        line.SetPosition(9, _verts[6]);    // set 2 -> 6
        line.SetPosition(10, _verts[5]);   // set 6 -> 5
        line.SetPosition(11, _verts[4]);   // set 5 -> 4
        line.SetPosition(12, _verts[7]);   // set 4 -> 7
        line.SetPosition(13, _verts[6]);   // set 7 -> 6
        line.SetPosition(14, _verts[2]);   // set 6 -> 2
        line.SetPosition(15, _verts[3]);   // set 2 -> 3
        line.SetPosition(16, _verts[7]);   // set 3 -> 7
    }
}
