using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class TerrainBurst : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material instancedMaterial;

    [SerializeField] private Color snowColour;
    [SerializeField] private Color rockColour;
    [SerializeField] private Color grassColour;
    [SerializeField] private Color waterColour;
    
    [Header("Height Values")]
    [SerializeField] private float snowHeight;
    [SerializeField] private float snowHeightRange;
    [SerializeField] private float rockHeight;
    [SerializeField] private float rockHeightRange;
    [SerializeField] private float grassHeight;
    [SerializeField] private float grassHeightRange;
    [SerializeField] private float waterHeight;
    
    [Header("Terrain")]
    [SerializeField] private int gridSize = 500;
    [SerializeField] private float noiseScale;
    [SerializeField] private float heightMult;
    [SerializeField] private float heightOffset;

    private Mesh _cubeMesh;
    private GraphicsBuffer _commandBuffer;
    private GraphicsBuffer _positionBuffer;
    private RenderParams _renderParams;
    private bool _isInitialized;

    private float _seed;

    // Fixed layout explicitly configured for Burst processing
    public struct CellData
    {
        public Vector4 positionScale;
        public Color colour;
    }

    // Burst-compiled parallel processing structural design
    [BurstCompile(CompileSynchronously = true)]
    private struct GenerateTerrainJob : IJobFor
    {
        [WriteOnly] public NativeArray<CellData> OutputCells;
        
        public int GridSize;
        public float Seed;
        public float NoiseScale;
        public float HeightMult;
        public float HeightOffset;
        public float WaterHeight;
        public float SnowHeight;
        public float SnowHeightRange;
        public float RockHeight;
        public float RockHeightRange;
        public float GrassHeight;
        public float GrassHeightRange;

        public Color SnowColour;
        public Color RockColour;
        public Color GrassColour;
        public Color WaterColour;

        public void Execute(int index)
        {
            // Reverse coordinates out of flat 1D index
            int x = index / GridSize;
            int z = index % GridSize;

            var xVal = ((float)x / GridSize - 1) + Seed;
            var yVal = ((float)z / GridSize - 1) + Seed;
            
            // Unity.Mathematics math API is required for Burst compatibility
            var randVal = Unity.Mathematics.noise.cnoise(new Unity.Mathematics.float2(xVal * NoiseScale, yVal * NoiseScale));
            
            // Map cnoise range (-1 to 1) to standard Perlin configuration spacing (0 to 1)
            randVal = (randVal + 1f) * 0.5f;

            var height = randVal * HeightMult + HeightOffset;
            if (height < WaterHeight) height = WaterHeight;

            CellData data;
            data.positionScale = new Vector4(x, height, z, 1.0f);
            
            // System pseudorandomization tracking variant for Burst (Random.Range is not thread-safe)
            uint state = (uint)(index + 1) * 0x9E3779B9; 
            float prngSnow = GetRandomRange(-SnowHeightRange, SnowHeightRange, ref state);
            float prngRock = GetRandomRange(-RockHeightRange, RockHeightRange, ref state);
            float prngGrass = GetRandomRange(-GrassHeightRange, GrassHeightRange, ref state);

            if (height > SnowHeight + prngSnow) data.colour = SnowColour;
            else if (height > RockHeight + prngRock) data.colour = RockColour;
            else if (height > GrassHeight + prngGrass) data.colour = GrassColour;
            else data.colour = WaterColour;

            OutputCells[index] = data;
        }

        // LCG PRNG algorithm compatible with native threads
        private float GetRandomRange(float min, float max, ref uint state)
        {
            state = state * 1664525 + 1013904223;
            float fraction = (float)state / uint.MaxValue;
            return min + fraction * (max - min);
        }
    }

    void Start()
    {
        _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        _seed = Random.Range(0f, 1000f);
        GenerateTerrain();
    }

    void Update()
    {
        if (!_isInitialized) return;
        Graphics.RenderMeshIndirect(_renderParams, _cubeMesh, _commandBuffer);
    }
    
    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (!Application.isPlaying) return;
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        CleanUpBuffers();

        int totalCells = gridSize * gridSize;
        
        // NativeArray provides direct thread access safe memory buffer pointers
        NativeArray<CellData> cellNativeArray = new NativeArray<CellData>(totalCells, Allocator.TempJob);

        GenerateTerrainJob terrainJob = new GenerateTerrainJob
        {
            OutputCells = cellNativeArray,
            GridSize = gridSize,
            Seed = _seed,
            NoiseScale = noiseScale,
            HeightMult = heightMult,
            HeightOffset = heightOffset,
            WaterHeight = waterHeight,
            SnowHeight = snowHeight,
            SnowHeightRange = snowHeightRange,
            RockHeight = rockHeight,
            RockHeightRange = rockHeightRange,
            GrassHeight = grassHeight,
            GrassHeightRange = grassHeightRange,
            SnowColour = snowColour,
            RockColour = rockColour,
            GrassColour = grassColour,
            WaterColour = waterColour
        };

        // Schedule operations over all available CPU logical cores concurrently
        JobHandle jobHandle = terrainJob.ScheduleParallel(totalCells, 64, default);
        jobHandle.Complete(); // Block execution path until background threads settle array payload

        // Allocate Graphics buffer directly from NativeArray data payload pointer safely
        _positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalCells, sizeof(float) * 8);
        _positionBuffer.SetData(cellNativeArray);
        
        // Free structural temporary container bounds immediately
        cellNativeArray.Dispose();

        instancedMaterial.SetBuffer("_CellDataBuffer", _positionBuffer);

        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        GraphicsBuffer.IndirectDrawIndexedArgs[] args = new GraphicsBuffer.IndirectDrawIndexedArgs[1]
        {
            new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = _cubeMesh.GetIndexCount(0),
                instanceCount = (uint)totalCells,
                startIndex = _cubeMesh.GetIndexStart(0),
                baseVertexIndex = _cubeMesh.GetBaseVertex(0),
                startInstance = 0
            }
        };
        _commandBuffer.SetData(args);

        _renderParams = new RenderParams(instancedMaterial)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * gridSize * heightMult),
            receiveShadows = true,
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
        };

        _isInitialized = true;
    }

    private void CleanUpBuffers()
    {
        if (_positionBuffer != null) { _positionBuffer.Release(); _positionBuffer = null; }
        if (_commandBuffer != null) { _commandBuffer.Release(); _commandBuffer = null; }
        _isInitialized = false;
    }

    void OnDestroy() => CleanUpBuffers();
}