using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    private VoxelGrid _data;
    private BlockManager _blocks;

    private List<Vector3> _meshVert = new List<Vector3>();
    private List<int> _meshTri = new List<int>();
    private List<Vector2> _meshUV = new List<Vector2>();
    private Vector3[] face = new Vector3[4];
    private Vector2[] faceUV = new Vector2[4];
    private int faceCount = 0;

    public MeshGenerator(VoxelGrid data, BlockManager blocks)
    {
        _data = data;
        _blocks = blocks;
    }

    /// <summary>
    /// Updates the mesh for a section of the voxel grid
    /// </summary>
    /// <param name="bounds">The area to generate data</param>
    /// <param name="offset">The offset in the voxel grid</param>
    /// <param name="scale">The size multiplier for each face</param>
    /// <param name="mesh">The referenced Mesh to update</param>
    public void BuildMesh(Vector3Int bounds, Vector3Int offset, float scale, ref Mesh mesh)
    {
        byte index;
        Block block;
        Vector3Int dataPoint = Vector3Int.zero;
        Vector3Int chunkPoint = Vector3Int.zero;
        Vector3Int Vector3Int_front = new Vector3Int(0,0,1);
        Vector3Int Vector3Int_back = new Vector3Int(0,0,-1);

        // generate faces (adjacent to air) for all solid blocks
        for (int x = 0; x < bounds.x; x++)
        {
            for (int z = 0; z < bounds.z; z++)
            {
                for (int y = 0; y < bounds.y; y++)
                {
                    chunkPoint.x = x; chunkPoint.y = y; chunkPoint.z = z;
                    dataPoint = chunkPoint + offset;

                    index = _data.Get(dataPoint);
                    if (index == 0) { continue; }

                    block = _blocks.library[index];
                    if (_data.Get(dataPoint + Vector3Int.up) == 0)    { AddFace(chunkPoint, block, scale, Cube.Direction.Top);    }
                    if (_data.Get(dataPoint + Vector3Int.down) == 0)  { AddFace(chunkPoint, block, scale, Cube.Direction.Bottom); }
                    if (_data.Get(dataPoint + Vector3Int_front) == 0) { AddFace(chunkPoint, block, scale, Cube.Direction.North);  }
                    if (_data.Get(dataPoint + Vector3Int.right) == 0) { AddFace(chunkPoint, block, scale, Cube.Direction.East);   }
                    if (_data.Get(dataPoint + Vector3Int_back) == 0)  { AddFace(chunkPoint, block, scale, Cube.Direction.South);  }
                    if (_data.Get(dataPoint + Vector3Int.left) == 0)  { AddFace(chunkPoint, block, scale, Cube.Direction.West);   }
                }
            }
        }

        // set the mesh with the new values
        mesh.Clear();
        mesh.vertices = _meshVert.ToArray();
        mesh.triangles = _meshTri.ToArray();
        mesh.uv = _meshUV.ToArray();
        mesh.RecalculateNormals();

        // cleanup the cache
        _meshVert.Clear();
        _meshTri.Clear();
        _meshUV.Clear();
        faceCount = 0;
    }

    /// <summary>
    /// Add a cube face to the generators data cache
    /// </summary>
    /// <param name="point">The point in the voxel grid</param>
    /// <param name="block">The block to be rendered</param>
    /// <param name="scale">The size multiplier for the cube face</param>
    /// <param name="dir">The direction of the cube face</param>
    private void AddFace(Vector3Int point, Block block, float scale, Cube.Direction dir)
    {
        float size = _blocks.texture.size;
        Vector2 texture = block.textures[dir];
        float halfScale = scale * 0.5f;

        Vector3 position = point;
        position *= scale;

        Cube.Face((int)dir, position, halfScale, ref face);
        _meshVert.AddRange(face);

        // add uv coordinates for the face
        faceUV[0].x = size * texture.x + size;
        faceUV[0].y = size * texture.y;
        faceUV[1].x = size * texture.x + size;
        faceUV[1].y = size * texture.y + size;
        faceUV[2].x = size * texture.x;
        faceUV[2].y = size * texture.y + size;
        faceUV[3].x = size * texture.x;
        faceUV[3].y = size * texture.y;
        _meshUV.AddRange(faceUV);

        // add triangles for the face
        _meshTri.Add(faceCount * 4);      // 1
        _meshTri.Add(faceCount * 4 + 1);  // 2
        _meshTri.Add(faceCount * 4 + 2);  // 3
        _meshTri.Add(faceCount * 4);      // 1
        _meshTri.Add(faceCount * 4 + 2);  // 3
        _meshTri.Add(faceCount * 4 + 3);  // 4
        faceCount++;
    }
}
