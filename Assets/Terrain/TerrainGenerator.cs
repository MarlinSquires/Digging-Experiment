using Unity.VisualScripting;
using UnityEngine;

// Terrain generator using DOD instead of classic OOP
// Height is determined through perlin noise

public class TerrainGenerator : MonoBehaviour
{

    [SerializeField] private GameObject cubePrefab;

    
    [Header("Grid Settings")] 
    [Tooltip("Length in x-axis")] [Min(1)] [SerializeField] private int gridWidth;
    [Tooltip("Length in y-axis")] [Min(1)] [SerializeField] private int gridHeight; // length in y axis
    [Tooltip("Length in z-axis")] [Min(1)] [SerializeField] private int gridLength; // length in z axis
    [Space(15)]
    
    [Min(0f)] [SerializeField] private float heightScale;
    [SerializeField] private float heightOffset;
    [Space(15)]
    
    [SerializeField] private float noiseScaleX = 1f;
    [SerializeField] private float noiseScaleZ = 1f;

    
    [Header("Material Values")] 
    [SerializeField] private Material snowMaterial;
    [SerializeField] private int snowHeight;
    [Min(0)] [SerializeField] private int snowHeightRange;
    [Space(15)]
    
    [SerializeField] private Material rockMaterial;
    [SerializeField] private int rockHeight;
    [Min(0)] [SerializeField] private int rockHeightRange;
    [Space(15)]
    
    [SerializeField] private Material grassMaterial;
    [SerializeField] private int grassHeight;
    [Min(0)] [SerializeField] private int grassHeightRange;
    [Space(15)]
    
    [SerializeField] private Material waterMaterial;
    [SerializeField] private int waterHeight;
    [Min(0)] [SerializeField] private int waterHeightRange;


    private GameObject[] _grid;
    
    
    void Start()
    {
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
        
        
        var arrayLength = gridLength * gridWidth * gridHeight;
        _grid = new GameObject[arrayLength];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridLength; z++)
            {
                // Determine height based on perlin noise
                float noiseVal = Mathf.PerlinNoise((float)x / gridWidth * noiseScaleX, (float)z / gridLength * noiseScaleZ);
                int height = (int)(noiseVal * heightScale);
                
                if (height > gridHeight) height = gridHeight; 
                
                for (int y = 0; y < height; y++)
                {
                    int index = x + (z * gridLength) + (y * gridLength * gridWidth);
                    
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
