using TMPro;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    
    
    [Header("Block Spawn Heights")]
    [SerializeField] private int grassHeight;
    [SerializeField] private int grassHeightRange;
    public int GrassHeight => grassHeight;
    public int GrassHeightRange => grassHeightRange;
    [Space(15)]

    [SerializeField] private int dirtHeight;
    [SerializeField] private int dirtHeightRange;
    public int DirtHeight => dirtHeight;
    public int DirtHeightRange => dirtHeightRange;
    [Space(15)]

    [SerializeField] private int stoneHeight;
    [SerializeField] private int stoneHeightRange;
    public int StoneHeight => stoneHeight;
    public int StoneHeightRange => stoneHeightRange;
    [Space(15)]

    [SerializeField] private int bedrockHeight;
    [SerializeField] private int bedrockHeightRange;
    public int BedrockHeight => bedrockHeight;
    public int BedrockHeightRange => bedrockHeightRange;
    [Space(15)]
    
    
    
    
    [SerializeField] private Material material;
    public Material GetMaterial() => material;

    public BlockType[] blockTypes;

    private Chunk[] _chunks;


    void Start()
    {
        player = Camera.main.transform;
        
        GenerateWorld();
        
    }

    void GenerateWorld()
    {
        _chunks = new Chunk[2048];
        
        
        int index = 0;
        for (int x = 0; x < WorldData.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldData.WorldSizeInChunks; z++)
            {
                _chunks[index] = CreateChunk(x, z);
                index++;
            }
        }
    }

    // Return voxel ID based on position
    // Used to generate different block types
    public byte GetVoxel(Vector3 pos)
    {
        //if (!IsVoxelInWorld(pos)) return 0;
        
        switch (pos.y)
        {
            case var h when h > GrassHeight + Random.Range(-GrassHeightRange, GrassHeightRange):
                return 4;
            case var h when h > DirtHeight + Random.Range(-DirtHeightRange, DirtHeightRange):
                return 3;
            case var h when h > StoneHeight + Random.Range(-StoneHeightRange, StoneHeightRange):
                return 2;
            case var h when h >= BedrockHeight + Random.Range(-BedrockHeightRange, BedrockHeightRange):
                return 1;
            default:
                Debug.LogWarning("Block spawn height out of range!");
                return 0;
        }
        
    }

    bool IsChunkInworld(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < WorldData.WorldSizeInChunks  && 
               coord.y >= 0 && coord.y < WorldData.WorldSizeInChunks;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        return 
            pos.x >= 0 && pos.x < (WorldData.WorldSizeInChunks) * ChunkData.ChunkSize && 
            pos.y >= 0 && pos.y < ChunkData.ChunkHeight && 
            pos.z >= 0 && pos.z < (WorldData.WorldSizeInChunks) * ChunkData.ChunkSize;
    }
    

    Chunk CreateChunk(int x, int z)
    {
        return new Chunk(this, new Vector2Int(x, z));
    }


    public Chunk GetChunk(int x, int z)
    {
        int index = x + z * WorldData.WorldSizeInChunks;
        return _chunks[index];
    }
    
    
    
    
} // Class ends here




[System.Serializable]
public class BlockType
{
    public string name;
    public bool opaque;

    [Header("Face Textures")] 
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Used to assign the face ID (using atlas index) to the correct face
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            // The order matters here, must be in the same order as faces are declared in VoxelData
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.LogWarning("Invalid face index");
                return -1;
        }
    }

}
