using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
   
    private Vector3Int _chunkPosition;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    
    private int _triangleIndex = 0;
    private List<Vector3> _vertices = new();
    private List<int> _triangles = new();
    private List<Vector2> _uvs = new();

    private bool[,,] voxelMap = new bool[ChunkData.ChunkSize, ChunkData.ChunkHeight, ChunkData.ChunkSize];
    

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        
        PopulateVoxelMap();
        CreateMeshData(transform.position);
        CreateMesh();
    }

    
    
    void PopulateVoxelMap()
    {
        for (int y = 0; y < ChunkData.ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkData.ChunkSize; z++)
                {
                    voxelMap[x, y, z] = true;
                }
            }
        }
    }
  
    
    void CreateMeshData(Vector3 chunkPos)
    {
        for (int y = 0; y < ChunkData.ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkData.ChunkSize; z++)
                {
                    AddVoxelData(chunkPos + new Vector3(x, y, z));
                }
            }
        }
    }
    
    
    void AddVoxelData(Vector3 voxelPos)
    {
        for (int face = 0; face < 6; face++)
        {
            if (CheckVoxel(voxelPos + VoxelData.FaceNormals[face])) continue;

            for (int j = 0; j < 4; j++)
            {
                int triIndex = VoxelData.VoxelTris[face, j];
                _vertices.Add(VoxelData.VoxelVerts[triIndex] + voxelPos);
                _uvs.Add(VoxelData.FaceUVs[j]);
            }

            for (int j = 0; j < 6; j++)
            {
                _triangles.Add(_triangleIndex + VoxelData.VertOrder[j]);
                
            }

            _triangleIndex += 4;
        }
    }
    
    


    
    
    void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = _triangles.ToArray(),
            uv = _uvs.ToArray()
        };

        mesh.RecalculateNormals();
        
        _meshFilter.mesh = mesh;
    }


    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > ChunkData.ChunkSize - 1 || 
            y < 0 || y > ChunkData.ChunkHeight - 1 || 
            z < 0 || z > ChunkData.ChunkSize - 1)
            return false;
        
        
        return voxelMap[x, y, z];
    }
    
    
    
}
