using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class Chunk 
{
   
    private GameObject _chunkObject; // Chunk is not a monobehaviour, so it spawns a chunkObject that holds the meshRenderer and meshFilter
    public Vector2Int coord;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private World _world;

    
    private int _triangleIndex;
    private readonly List<Vector3> _vertices = new(ChunkData.ChunkSize * ChunkData.ChunkSize * ChunkData.ChunkHeight * 8);
    private readonly List<int> _triangles = new();
    private readonly List<Vector2> _uvs = new();

    // This map holds the blockType (byte) of each voxel in the chunk - flat array corresponds to 3D space
    private readonly byte[] _voxelMap = new byte[ChunkData.ChunkSize * ChunkData.ChunkHeight * ChunkData.ChunkSize];

    
    public bool IsActive
    {
        get => _chunkObject.activeSelf;
        set => _chunkObject.SetActive(value);
    }

    public Vector3 Position => _chunkObject.transform.position;


    public Chunk(World world, Vector2Int chunkCoord)
    {
        // Init values
        _world = world;
        coord = chunkCoord;
        
        
        // Create chunkObject
        _chunkObject = new GameObject($"Chunk: {coord.x}, {coord.y}");
        _chunkObject.transform.SetParent(_world.transform);
        _chunkObject.transform.position = new Vector3((coord.x - WorldData.WorldSizeInChunks / 2) * ChunkData.ChunkSize, 0, (coord.y - WorldData.WorldSizeInChunks / 2) * ChunkData.ChunkSize);
        
        _meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _meshRenderer.material = _world.GetMaterial();
        
        
        // Create chunk data & mesh
        PopulateVoxelMap();
        PopulateVoxelMap();
        CreateChunkMeshData();
        CreateMesh();
    }
    
    
    
    // This voxel
    void PopulateVoxelMap()
    {
        for (int y = 0; y < ChunkData.ChunkHeight; y++)
        {
            for (int z = 0; z < ChunkData.ChunkSize; z++)
            {
                for (int x = 0; x < ChunkData.ChunkSize; x++)
                {
                    _voxelMap[Pos3dToIndex(new Vector3(x, y, z))] = _world.GetVoxel(new Vector3Int(x, y, z) + Position);
                }
            }
        }
    }
  
    // Builds the chunk mesh from each constituent voxel
    void CreateChunkMeshData()
    {
        for (int y = 0; y < ChunkData.ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkData.ChunkSize; z++)
                {
                    AddVoxelMeshData(new Vector3(x, y, z));
                }
            }
        }
    }
    
    // Adds the mesh data (verts, tris, normals, uvs) for a single voxel
    void AddVoxelMeshData(Vector3 voxelPos)
    {
        int index = Pos3dToIndex(voxelPos);
        byte blockID = _voxelMap[index];
        
        var blockType = _world.blockTypes[blockID];
        
        for (int face = 0; face < 6; face++)
        {
            if (CheckVoxel(voxelPos + VoxelData.FaceNormals[face])) continue;
            
            // Add vertices
            int faceIndexOffset = face * 4;
            // Explicit repetition instead of nested loops improves performance
            _vertices.Add(VoxelData.VoxelVerts[VoxelData.FaceVertexIndices[faceIndexOffset]] + voxelPos); 
            _vertices.Add(VoxelData.VoxelVerts[VoxelData.FaceVertexIndices[faceIndexOffset + 1]] + voxelPos);
            _vertices.Add(VoxelData.VoxelVerts[VoxelData.FaceVertexIndices[faceIndexOffset + 2]] + voxelPos);
            _vertices.Add(VoxelData.VoxelVerts[VoxelData.FaceVertexIndices[faceIndexOffset + 3]] + voxelPos);
            
            AddTexture(blockType.GetTextureID(face));
            
            // Add triangles
            _triangles.Add(_triangleIndex);
            _triangles.Add(_triangleIndex + 1);
            _triangles.Add(_triangleIndex + 2);
            _triangles.Add(_triangleIndex + 2);
            _triangles.Add(_triangleIndex + 1);
            _triangles.Add(_triangleIndex + 3);

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
    

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return x >= 0 && x < ChunkData.ChunkSize &&
               y >= 0 && y < ChunkData.ChunkHeight &&
               z >= 0 && z < ChunkData.ChunkSize;
    }
    
    
    // Checks whether or not to render voxel based on its neighbours. 
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);


        if (!IsVoxelInChunk(x, y, z)) return false;

        int voxelIndex = Pos3dToIndex(new Vector3(x, y, z));
        byte blockType = _voxelMap[voxelIndex];
        
        return _world.blockTypes[blockType].opaque;
    }

    
    // Adds a texture to 1 face of a voxel
    void AddTexture(int textureID)
    {
        var atlasSize = VoxelData.AtlasSizeInBlocks;
        var blockSize = VoxelData.normalizedBlockSize;
        
        float x = textureID % atlasSize;
        float y = textureID / atlasSize; // Truncation is intended

        x *= blockSize;
        y *= blockSize;
        
        y = 1f - y - blockSize; // Flip the y value, since the index starts in the top left, but UV 0,0 is in the bottom left
        
        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + blockSize));
        _uvs.Add(new Vector2(x + blockSize, y));
        _uvs.Add(new Vector2(x + blockSize, y + blockSize));
    }

    // Converts from an array index to a local vector3 pos
    Vector3 IndexTo3D(int index)
    {
        int width = ChunkData.ChunkSize;
        
        // Truncation is intended
        return new Vector3(
            index % width,              // x
            index / (width * width),    // y
            (index / width) % width     // z
        );
    }
    
    // Converts from a local vector3 voxel pos to an array index
    int Pos3dToIndex(Vector3 pos)
    {
        int width = ChunkData.ChunkSize;
        return (int)(pos.x + (pos.y * width * width) + (pos.z * width));
        
    }
    
    
}
