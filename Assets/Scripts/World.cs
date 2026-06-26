using TMPro;
using UnityEngine;

public class World : MonoBehaviour
{
    
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
        GenerateWorld();
        
    }

    void GenerateWorld()
    {
        _chunks = new Chunk[2048];

        int count = 4;
        
        int index = 0;
        for (int x = 0; x < count; x++)
        {
            for (int z = 0; z < count; z++)
            {
                _chunks[index] = new Chunk(this, new ChunkCoord(x, z));
                index++;
            }
        }
        
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
