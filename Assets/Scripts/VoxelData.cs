using UnityEngine;

// Holds lookup tables for voxel verts and tris

public static class VoxelData
{

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
    
    public static readonly int[,] VoxelTris = new int[6, 4]
    {
        { 0, 3, 1, 2 }, // Back face
        { 5, 6, 4, 7 }, // Front face
        { 3, 7, 2, 6 }, // Top face
        { 1, 5, 0, 4 }, // Bottom face
        { 4, 7, 0, 3 }, // Left face
        { 1, 2, 5, 6 }, // Right face
    };

    public static readonly int[] VertOrder = new int[6]
    {
        0, 1, 2, 2, 1, 3
    };
    
    // Faces are made up of 2 tris
    public static readonly Vector3[] FaceNormals = new Vector3[6]
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
    };
    
    public static readonly Vector2[] FaceUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };





}
