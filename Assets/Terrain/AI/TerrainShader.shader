Shader "Custom/DOD_InstancedShader"
{
    Properties
    {
        // Properties can be left empty
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // CRITICAL: Enables instancing variant compilation loops
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                uint instanceID     : SV_InstanceID; // Built-in GPU structural index
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
            };

            // Layout matches C# struct memory exactly (32 bytes per cell)
            struct CellData
            {
                float4 positionScale;
                float4 colour; // Synced name mapping
            };

            // Directly exposed buffer mapping (Populated via material.SetBuffer)
            StructuredBuffer<CellData> _CellDataBuffer;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Allow engine to map target architecture instancing tables
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Fetch instance payload using the direct GPU ID
                CellData cell = _CellDataBuffer[input.instanceID];

                // Calculate custom vertex positions based on data bounds
                float3 localPos = input.positionOS.xyz * cell.positionScale.w; // Multiply vertex by Scale factor
                float3 worldPos = localPos + cell.positionScale.xyz;          // Add world translation coordinates

                // Transform the adjusted positions to Clip Space
                output.positionCS = TransformWorldToHClip(worldPos);
                output.color = cell.colour;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Directly write vertex layout color tracking data to pixel color
                return input.color;
            }
            ENDHLSL
        }
    }
}