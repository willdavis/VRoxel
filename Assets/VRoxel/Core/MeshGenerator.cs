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
        private Vector3[] _face = new Vector3[4];
        private Vector2[] _faceUV = new Vector2[4];
        private int _faceCount = 0;

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
            Vector3Int voxel = Vector3Int.zero;
            Vector3 position = Vector3.zero;
            bool hasBlock;
            Block block;
            byte index;

            mesh.Clear();

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

                        // if no block data is present, exit and do not render the mesh
                        hasBlock = _blocks.library.TryGetValue(index, out block);
                        if(!hasBlock)
                        {
                            Debug.LogAssertion("Chunk failed to render: no block found with index:" + index);
                            ClearCache();
                            return;
                        }

                        if (_data.Get(voxel + Direction3Int.Up) == 0)    { AddFace(position, block, Cube.Direction.Top);    }
                        if (_data.Get(voxel + Direction3Int.Down) == 0)  { AddFace(position, block, Cube.Direction.Bottom); }
                        if (_data.Get(voxel + Direction3Int.North) == 0) { AddFace(position, block, Cube.Direction.North);  }
                        if (_data.Get(voxel + Direction3Int.East) == 0)  { AddFace(position, block, Cube.Direction.East);   }
                        if (_data.Get(voxel + Direction3Int.South) == 0) { AddFace(position, block, Cube.Direction.South);  }
                        if (_data.Get(voxel + Direction3Int.West) == 0)  { AddFace(position, block, Cube.Direction.West);   }
                    }
                }
            }

            mesh.vertices = _meshVert.ToArray();
            mesh.triangles = _meshTri.ToArray();
            mesh.uv = _meshUV.ToArray();
            mesh.RecalculateNormals();
            ClearCache();
        }

        private void ClearCache()
        {
            _meshVert.Clear();
            _meshTri.Clear();
            _meshUV.Clear();
            _faceCount = 0;
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
            Cube.Face((int)dir, position, _halfScale, ref _face);
            _meshVert.AddRange(_face);

            // add uv coordinates for the face
            _faceUV[0].x = size * texture.x + size;
            _faceUV[0].y = size * texture.y;
            _faceUV[1].x = size * texture.x + size;
            _faceUV[1].y = size * texture.y + size;
            _faceUV[2].x = size * texture.x;
            _faceUV[2].y = size * texture.y + size;
            _faceUV[3].x = size * texture.x;
            _faceUV[3].y = size * texture.y;
            _meshUV.AddRange(_faceUV);

            // add triangles for the face
            _meshTri.Add(_faceCount * 4);      // 1
            _meshTri.Add(_faceCount * 4 + 1);  // 2
            _meshTri.Add(_faceCount * 4 + 2);  // 3
            _meshTri.Add(_faceCount * 4);      // 1
            _meshTri.Add(_faceCount * 4 + 2);  // 3
            _meshTri.Add(_faceCount * 4 + 3);  // 4
            _faceCount++;
        }
    }
}
