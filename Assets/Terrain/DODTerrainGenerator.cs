using UnityEngine;

// Voxel-based terrain generator using OOP - highly inefficient
// Height is determined through perlin noise

public class DODTerrainGenerator : MonoBehaviour
{
    
    [Header("Chunk Values")] 
    [Tooltip("Number of chunks to spawn")] 
    [Min(1)] [SerializeField] private int chunkCount = 2;
    [Tooltip("Length and width - x and z axes")] 
    [Min(2)] [SerializeField] private int chunkSize = 16;
    [Tooltip("Height in y-axis")] 
    [Min(2)] [SerializeField] private int chunkHeight = 16; // length in y axis
    
    
    [Header("Noise Values")] 
    [Min(0f)] [SerializeField] private float noiseScaleX = 1f;
    [Min(0f)] [SerializeField] private float noiseScaleZ = 1f;
    
    
    [Header("Materials")] 
    [SerializeField] private Color snowColor;
    [SerializeField] private Color rockColor;
    [SerializeField] private Color grassColor;
    [SerializeField] private Color waterColor;
    
    
    [Header("Height Values")] 
    [Min(0f)] [SerializeField] private float heightScale;
    [SerializeField] private float heightOffset;
    [Space(15)]
    
    [SerializeField] private int snowHeight;
    [Min(0)] [SerializeField] private int snowHeightRange;
    [Space(15)]
    
    [SerializeField] private int rockHeight;
    [Min(0)] [SerializeField] private int rockHeightRange;
    [Space(15)]
    
    [SerializeField] private int grassHeight;
    [Min(0)] [SerializeField] private int grassHeightRange;
    [Space(15)]
    
    [SerializeField] private int waterHeight;
    [Min(0)] [SerializeField] private int waterHeightRange;

    private Mesh _cubeMesh;

    private GameObject[] _grid;

    private struct Chunk
    {
        public Block[] blocks;
        public int chunkID;

        public Chunk(int id)
        {
            chunkID = id;
        }
    }

    private struct Block
    {
        public Vector3 position;
        public BlockType blockType;
    }

    private enum BlockType
    {
        Snow,
        Rock,
        Grass,
        Water
    }
    
    
    void Start()
    {
        _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        GenerateTerrain();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (!Application.isPlaying) return;
        GenerateTerrain();
    }


    void GenerateTerrain()
    {
        if (_grid != null)
        {
            foreach (var go in _grid)
            {
                Destroy(go);
            }

            _grid = null;
        }
        
        
        var arrayLength = gridLength * chunkWidth * chunkHeight;
        _grid = new GameObject[arrayLength];

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                // Determine height based on perlin noise
                float noiseVal = Mathf.PerlinNoise((float)x / chunkWidth * noiseScaleX, (float)z / gridLength * noiseScaleZ);
                int height = (int)(noiseVal * heightScale);
                
                if (height > chunkHeight) height = chunkHeight; 
                
                for (int y = 0; y < height; y++)
                {
                    int index = x + (z * gridLength) + (y * gridLength * chunkWidth);
                    
                    _grid[index] = Instantiate(cubePrefab, new Vector3(x, y, z), Quaternion.identity);
                    
                    SetColour(_grid[index], y);
                    
                }
            }
        }
    }



    void SetColour(GameObject cube, int height)
    {
        switch (height)
        {
            case var h when h > snowHeight + Random.Range(-snowHeightRange, snowHeightRange):
                cube.GetComponent<Renderer>().material = snowMaterial;
                break;
            case var h when h > rockHeight + Random.Range(-rockHeightRange, rockHeightRange):
                cube.GetComponent<Renderer>().material = rockMaterial;
                break;
            case var h when h > grassHeight + Random.Range(-grassHeightRange, grassHeightRange):
                cube.GetComponent<Renderer>().material = grassMaterial;
                break;
            case var h when h >= waterHeight + Random.Range(-waterHeightRange, waterHeightRange):
                cube.GetComponent<Renderer>().material = waterMaterial;
                break;
        }
    }




}
