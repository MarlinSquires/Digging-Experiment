using System.Collections.Generic;
using UnityEngine;


public class Chunk 
{
   
    private GameObject _chunkObject; // Chunk is not a monobehaviour, so it spawns a chunkObject that holds the meshRenderer and meshFilter
    public ChunkCoord coord;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private World _world;

    
    private int _triangleIndex;
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();
    private readonly List<Vector2> _uvs = new();

    // This map holds the blockType (byte) of each voxel at a given position
    private readonly byte[,,] _voxelMap = new byte[ChunkData.ChunkSize, ChunkData.ChunkHeight, ChunkData.ChunkSize];



    public Chunk(World world, ChunkCoord chunkCoord)
    {
        // Init values
        _world = world;
        coord = chunkCoord;
        
        
        // Create chunkObject
        _chunkObject = new GameObject($"Chunk: {coord.x}, {coord.z}");
        _chunkObject.transform.SetParent(_world.transform);
        _chunkObject.transform.position = new Vector3(coord.x * ChunkData.ChunkSize, 0, coord.z * ChunkData.ChunkSize);
        
        _meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _meshRenderer.material = _world.GetMaterial();
        
        
        // Create chunk data & mesh
        PopulateVoxelMap();
        PopulateVoxelMap();
        CreateMeshData();
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
                    _voxelMap[x, y, z] = ChooseBlockType((byte)y);
                }
            }
        }
    }
  
    
    void CreateMeshData()
    {
        for (int y = 0; y < ChunkData.ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (int z = 0; z < ChunkData.ChunkSize; z++)
                {
                    AddVoxelData(new Vector3(x, y, z));
                }
            }
        }
    }
    
    
    void AddVoxelData(Vector3 voxelPos)
    {
        for (int face = 0; face < 6; face++)
        {
            if (CheckVoxel(voxelPos + VoxelData.FaceNormals[face])) continue;
            
            byte blockID = _voxelMap[(int)voxelPos.x, (int)voxelPos.y, (int)voxelPos.z];

            for (int j = 0; j < 4; j++)
            {
                int triIndex = VoxelData.VoxelTris[face, j];
                _vertices.Add(VoxelData.VoxelVerts[triIndex] + voxelPos);
            }

            AddTexture(_world.blockTypes[blockID].GetTextureID(face));

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
        
        
        return _world.blockTypes[_voxelMap[x, y, z]].opaque;
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


    byte ChooseBlockType(byte blockHeight)
    {
        switch (blockHeight)
        {
            case var h when h > _world.GrassHeight + Random.Range(-_world.GrassHeight, _world.GrassHeight):
                return 4;
            case var h when h > _world.DirtHeight + Random.Range(-_world.DirtHeight, _world.DirtHeight):
                return 3;
            case var h when h > _world.StoneHeight + Random.Range(-_world.StoneHeight, _world.StoneHeight):
                return 2;
            case var h when h > _world.BedrockHeight + Random.Range(-_world.BedrockHeight, _world.BedrockHeight):
                return 1;
            default:
                Debug.LogWarning("Block spawn height out of range!");
                return 0;
        }
    }
    
    
    
}


public struct ChunkCoord
{
    public int x;
    public int z;
    
    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

}
