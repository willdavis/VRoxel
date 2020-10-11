using UnityEngine;
using VRoxel.Core;

/// <summary>
/// Allows the attached GameObject to draw a bounding box around voxels.
/// </summary>
public class BlockCursor : MonoBehaviour
{
    GameObject cube, sphere;

    public enum Shape { Cuboid, Spheroid }

    public void UpdateCuboid(World world, Vector3 p1, Vector3 p2, float size = 1f)
    {
        if (!cube.activeSelf) { cube.SetActive(true); sphere.SetActive(false); }

        Vector3 cubeScale = Vector3.one;
        cubeScale.x = world.scale * size;
        cubeScale.y = world.scale * size;
        cubeScale.z = world.scale * size;

        if (p1 == p2)
        {
            // draw a cube
            transform.rotation = world.transform.rotation;
            transform.localScale = cubeScale;
            transform.position = p1;
        }
        else
        {
            // draw a rectangle
            Vector3Int v1 = world.SceneToGrid(p1);
            Vector3Int v2 = world.SceneToGrid(p2);

            float lengthX = Mathf.Abs(v1.x - v2.x);
            float lengthY = Mathf.Abs(v1.y - v2.y);
            float lengthZ = Mathf.Abs(v1.z - v2.z);

            Vector3 rectangleScale = new Vector3(
                cubeScale.x * lengthX + cubeScale.x,
                cubeScale.y * lengthY + cubeScale.y,
                cubeScale.z * lengthZ + cubeScale.z
            );

            transform.rotation = world.transform.rotation;
            transform.localScale = rectangleScale;
            transform.position = Vector3.Lerp(p1, p2, 0.5f);
        }
    }

    public void UpdateSpheroid(World world, Vector3 p1, Vector3 p2, float radius = 1f)
    {
        if (!sphere.activeSelf) { sphere.SetActive(true); cube.SetActive(false); }

        Vector3 sphereScale = Vector3.one;
        sphereScale.x = world.scale * radius;
        sphereScale.y = world.scale * radius;
        sphereScale.z = world.scale * radius;

        if (p1 == p2)
        {
            // draw sphere
            transform.rotation = world.transform.rotation;
            transform.localScale = sphereScale;
            transform.position = p1;
        }
    }

    void Awake()
    {
        cube = transform.GetChild(0).gameObject;
        sphere = transform.GetChild(1).gameObject;
    }
}