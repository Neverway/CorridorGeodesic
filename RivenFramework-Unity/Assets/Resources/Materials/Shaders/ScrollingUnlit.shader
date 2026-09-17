Shader "Custom/URP/ScrollingUnlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)

        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0

        _ScrollSpeedX("Scroll Speed X", Float) = 0.0
        _ScrollSpeedY("Scroll Speed Y", Float) = 0.0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0 // Off
        [Toggle] _ZWrite("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend [_SrcBlend] [_DstBlend]
        Cull [_Cull]
        ZWrite [_ZWrite]

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _EMISSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 emissionUV  : TEXCOORD1;
                float4 color       : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EmissionColor;
                float4 _EmissionMap_ST;
                float _ScrollSpeedX;
                float _ScrollSpeedY;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                float2 scrollOffset = float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;

                float2 uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv = uv + scrollOffset;

                float2 emissionUV = TRANSFORM_TEX(IN.uv, _EmissionMap);
                OUT.emissionUV = emissionUV + scrollOffset;

                OUT.color = IN.color * _BaseColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 col = texColor * IN.color;

                #if defined(_EMISSION)
                    half4 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.emissionUV);
                    col.rgb += emissionTex.rgb * _EmissionColor.rgb * texColor.a;
                #endif

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
