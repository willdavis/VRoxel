using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRoxel.Core
{
    public class MeshGenerator
    {
        private VoxelGrid _data;
        private BlockManager _blocks;
        private float _scale;
        private float _halfScale;

        private List<Vector3> _meshVert = new List<Vector3>();
        private List<int> _meshTri = new List<int>();
        private List<Vector2> _meshUV = new List<Vector2>();
        private Vector3[] face = new Vector3[4];
        private Vector2[] faceUV = new Vector2[4];
        private int faceCount = 0;

        public MeshGenerator(VoxelGrid data, BlockManager blocks, float scale)
        {
            _data = data;
            _blocks = blocks;
            _scale = scale;
            _halfScale = scale * 0.5f;
        }

        /// <summary>
        /// Updates the mesh for a section of the voxel grid
        /// </summary>
        /// <param name="size">The size of the voxel mesh to render</param>
        /// <param name="offset">The offset for the voxel data</param>
        /// <param name="mesh">The referenced Mesh to update</param>
        public void BuildMesh(Vector3Int size, Vector3Int offset, ref Mesh mesh)
        {
            byte index;
            Block block;
            Vector3 position = Vector3.zero;
            Vector3Int voxel = Vector3Int.zero;
            Vector3Int Vector3Int_front = new Vector3Int(0,0,1);
            Vector3Int Vector3Int_back = new Vector3Int(0,0,-1);

            // generate faces (adjacent to air) for all solid blocks
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        voxel.x = x;
                        voxel.y = y;
                        voxel.z = z;
                        voxel += offset;

                        index = _data.Get(voxel);
                        if (index == 0) { continue; }   // skip if the block is air

                        position.x = (float)x * _scale;
                        position.y = (float)y * _scale;
                        position.z = (float)z * _scale;

                        position.x -= 0.5f * ((float)size.x - 1f) * _scale;
                        position.y -= 0.5f * ((float)size.y - 1f) * _scale;
                        position.z -= 0.5f * ((float)size.z - 1f) * _scale;

                        block = _blocks.library[index]; // get the block metadata and check neighbors for air
                        if (_data.Get(voxel + Vector3Int.up) == 0)    { AddFace(position, block, Cube.Direction.Top);    }
                        if (_data.Get(voxel + Vector3Int.down) == 0)  { AddFace(position, block, Cube.Direction.Bottom); }
                        if (_data.Get(voxel + Vector3Int_front) == 0) { AddFace(position, block, Cube.Direction.North);  }
                        if (_data.Get(voxel + Vector3Int.right) == 0) { AddFace(position, block, Cube.Direction.East);   }
                        if (_data.Get(voxel + Vector3Int_back) == 0)  { AddFace(position, block, Cube.Direction.South);  }
                        if (_data.Get(voxel + Vector3Int.left) == 0)  { AddFace(position, block, Cube.Direction.West);   }
                    }
                }
            }

            // set the mesh with the new values
            mesh.Clear();
            mesh.vertices = _meshVert.ToArray();
            mesh.triangles = _meshTri.ToArray();
            mesh.uv = _meshUV.ToArray();
            mesh.RecalculateNormals();

            // clear the cache
            _meshVert.Clear();
            _meshTri.Clear();
            _meshUV.Clear();
            faceCount = 0;
        }

        /// <summary>
        /// Add cube face vertices, triangles, and UVs to the cache
        /// </summary>
        /// <param name="position">The position of the cube</param>
        /// <param name="block">The block data for the cube</param>
        /// <param name="dir">The direction of the cube to render</param>
        private void AddFace(Vector3 position, Block block, Cube.Direction dir)
        {
            float size = _blocks.texture.size;
            Vector2 texture = block.textures[dir];

            // add vertices for the face
            Cube.Face((int)dir, position, _halfScale, ref face);
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
}
