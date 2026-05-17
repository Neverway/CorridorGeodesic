Shader "SabreCSG/BrushPreview"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _GridAlpha("Grid Alpha", Range(0,1)) = 0.3
        [HideInInspector] _GridSize("Grid Size", float) = 1.0
        [HideInInspector] _GridToggle("Grid Toggle", float) = 1.0
        [HideInInspector] _FaceToggle("Face Toggle", float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            Fog { Mode Off }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _GridAlpha;
                float  _GridSize;
                float  _GridToggle;
                float  _FaceToggle;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float modGrid(float val, float m)
            {
                // Equivalent of the Legacy while-loop mod, but branchless
                val = val + m * 100.0 * ceil(max(0.0, -val) / m + 1.0);
                return fmod(val, m);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.worldPos    = posInputs.positionWS;
                OUT.worldNormal = normInputs.normalWS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float gridSize = max(0.01, _GridSize);

                float dist = max(0.01, distance(_WorldSpaceCameraPos, IN.worldPos));
                float len  = lerp(20.0, 140.0, min(1.0, gridSize));
                float m    = smoothstep(0.0, 1.0, dist / len);
                float gridThickness = max(0.03, m);

                gridThickness += lerp(
                    0.0,
                    lerp(log2(1.0 / gridSize) * 0.25, 0.0, min(1.0, gridSize)),
                    m
                );

                float4 c = _Color;
                c.a *= _FaceToggle;

                float3 worldNormal = abs(IN.worldNormal);

                worldNormal.x = (worldNormal.x > worldNormal.y && worldNormal.x > worldNormal.z) ? 1.0 : 0.0;
                worldNormal.y = (worldNormal.y > worldNormal.x && worldNormal.y > worldNormal.z) ? 1.0 : 0.0;
                worldNormal.z = (worldNormal.z > worldNormal.y && worldNormal.z > worldNormal.x) ? 1.0 : 0.0;

                float3 worldspace = IN.worldPos;
                worldspace -= (gridThickness * gridSize) / 6.28;

                float2 grid = float2(
                    (worldspace.z * worldNormal.x) + (worldspace.x * worldNormal.z) + (worldspace.x * worldNormal.y),
                    (worldspace.y * worldNormal.x) + (worldspace.y * worldNormal.z) + (worldspace.z * worldNormal.y)
                );

                grid.x = modGrid(grid.x, gridSize);
                grid.y = modGrid(grid.y, gridSize);

                grid.x = saturate(1.0 - sin(grid.x * ((3.14 / gridSize) + gridThickness / gridSize)) * (30.0 * gridSize));
                grid.y = saturate(1.0 - sin(grid.y * ((3.14 / gridSize) + gridThickness / gridSize)) * (30.0 * gridSize));

                float g = saturate(grid.x + grid.y);

                float3 emission = c.rgb + (g * _GridAlpha * _GridToggle);
                float  alpha    = c.a   + (g * _GridAlpha * _GridToggle);

                return half4(emission, saturate(alpha));
            }
            ENDHLSL
        }
    }

Fallback Off
}