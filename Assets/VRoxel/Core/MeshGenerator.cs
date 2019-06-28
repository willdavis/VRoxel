using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    private VoxelGrid _data;
    private BlockManager _blocks;

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
        
    }
}
