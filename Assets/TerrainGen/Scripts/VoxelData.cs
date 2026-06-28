using UnityEngine;

// Holds lookup tables for voxel verts and tris

public static class VoxelData
{

    public static readonly int AtlasSizeInBlocks = 4; // The number of blocks in the atlas

    public static float normalizedBlockSize => 1f / AtlasSizeInBlocks; // The portion of the UV map an individual block takes up, expressed as a float between 0 and 1


    // The 8 local-space positions for the 24 vertices of each cubic voxel
    public static readonly Vector3[] VoxelVerts = new Vector3[8]
    {
        new Vector3(0, 0, 0), // 0
        new Vector3(1, 0, 0), // 1
        new Vector3(1, 1, 0), // 2
        new Vector3(0, 1, 0), // 3
        
        new Vector3(0, 0, 1), // 4
        new Vector3(1, 0, 1), // 5
        new Vector3(1, 1, 1), // 6
        new Vector3(0, 1, 1), // 7
    };
    
    // While there are only 8 vert positions, each position is shared by 3 verts (1 for each connecting face), so the true vert count is 24
    public static readonly int[] FaceVertexIndices = new int[24]
    {
         0, 3, 1, 2 , // Back face
         5, 6, 4, 7 , // Front face
         3, 7, 2, 6 , // Top face
         1, 5, 0, 4 , // Bottom face
         4, 7, 0, 3 , // Left face
         1, 2, 5, 6   // Right face
    };
    
    // The normals for each of the 6 faces of the voxel
    public static readonly Vector3[] FaceNormals = new Vector3[6]
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
    };
    
    // The UV coords of the 4 verts of each face
    public static readonly Vector2[] FaceUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };





}
