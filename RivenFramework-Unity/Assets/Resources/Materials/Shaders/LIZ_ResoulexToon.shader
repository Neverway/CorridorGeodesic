Shader "Neverway/Resoulex Toon"
{
    Properties
    {
        [Header(Stencil Settings)][Space]
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.StencilOp)] _ZWrite ("ZWrite", Float) = 1
        _StencilRef       ("Stencil Ref",        Int)   = 0
        _StencilReadMask  ("Stencil Read Mask",   Int)   = 255
        _StencilWriteMask ("Stencil Write Mask",  Int)   = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp  ("Stencil Comp",  Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]       _StencilPass  ("Stencil Pass",  Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]       _StencilFail  ("Stencil Fail",  Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]       _StencilZFail ("Stencil ZFail", Float) = 0

        [Header(Material Settings)][Space]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2
        [KeywordEnum(TruePBR, StylizedPBR)] _SpecularMode ("Specular Mode", Float) = 0
        _AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
        _Color ("Color", Color) = (1,1,1,1)
        _AttenuationPowerrr ("Light Falloff", Range(0.1, 1.0)) = 0.34

        [Header(Main Texture Properties)][Space]
        _RampSmoothness ("Ramp Smoothness", Range(0.1, 1.0)) = 0.1
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Roughness Power", Range(0.0, 1.0)) = 0.5
        [NoScaleOffset] _SpecGlossMap ("Roughness Map", 2D) = "white" {}
        _Metallic ("Metallic Power", Range(0.0, 1.0)) = 0.0
        [NoScaleOffset] _MetallicGlossMap ("Metallic Map", 2D) = "white" {}
        _BumpScale ("Normal Power", Range(0.0, 1.0)) = 1.0
        [NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _Parallax ("Height Scale", Range(0, 0.08)) = 0
        [NoScaleOffset] _ParallaxMap ("Height Map", 2D) = "black" {}
        [NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.5

        [Header(Emission Properties)][Space]
        _EmissionDivision ("Emission Division", Range(1, 10)) = 1
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 0)
        [NoScaleOffset] _EmissionMap ("Emission", 2D) = "white" {}

        [Header(Detail Layer)][Space]
        _DetailAlbedoMap ("Detail Texture", 2D) = "black" {}
        _DetailProminence ("Detail Prominence", Range(0, 1)) = 0.2
        _DetailColor ("Detail Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Geometry"
        }
        LOD 200
        Cull [_CullMode]
        ZTest [_ZTest]
        ZWrite [_ZWrite]

        Stencil
        {
            Ref       [_StencilRef]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp      [_StencilComp]
            Pass      [_StencilPass]
            Fail      [_StencilFail]
            ZFail     [_StencilZFail]
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4  _MainTex_ST;
            float4  _DetailAlbedoMap_ST;
            half    _RampSmoothness;
            float   _AlphaClip;
            half    _Glossiness;
            half    _Metallic;
            half    _BumpScale;
            half    _Parallax;
            half    _EmissionDivision;
            half4   _EmissionColor;
            half    _SpecularStrength;
            half    _DetailProminence;
            half4   _DetailColor;
            float4  _Color;
            half _AttenuationPowerrr;
            float   _UseSlice;
            float   _ColorOnly;
            float3  _SliceCenterOne;
            float3  _SliceCenterTwo;
            float3  _SliceNormalOne;
            float3  _SliceNormalTwo;
        CBUFFER_END

        TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);
        TEXTURE2D(_SpecGlossMap);     SAMPLER(sampler_SpecGlossMap);
        TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_BumpMap);          SAMPLER(sampler_BumpMap);
        TEXTURE2D(_ParallaxMap);      SAMPLER(sampler_ParallaxMap);
        TEXTURE2D(_OcclusionMap);     SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_EmissionMap);      SAMPLER(sampler_EmissionMap);
        TEXTURE2D(_DetailAlbedoMap);  SAMPLER(sampler_DetailAlbedoMap);

        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _SPECULARMODE_TRUEPBR _SPECULARMODE_STYLIZEDPBR
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 uvDetail   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS     : SV_POSITION;
                float2 uv             : TEXCOORD0;
                float2 uvDetail       : TEXCOORD1;
                float3 positionWS     : TEXCOORD2;
                float3 normalWS       : TEXCOORD3;
                float4 tangentWS      : TEXCOORD4;
                float3 viewDirWS      : TEXCOORD5;
                float4 shadowCoord    : TEXCOORD6;
                half3  vertexLighting : TEXCOORD7;
                float3 viewDirTS      : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half luminance(half3 c) { return dot(c, half3(0.2126, 0.7152, 0.0722)); }

            // Parallax now takes tangent-space view direction
            float2 ComputeParallaxOffset(float height, float scale, float3 viewDirTS)
            {
                height = height * scale - scale * 0.5;
                float3 v = normalize(viewDirTS);
                v.z += 0.42;
                return height * (v.xy / v.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS  = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.tangentWS   = float4(nrmInputs.tangentWS, IN.tangentOS.w * GetOddNegativeScale());
                OUT.viewDirWS   = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv          = TRANSFORM_TEX(IN.uv,       _MainTex);
                OUT.uvDetail    = TRANSFORM_TEX(IN.uvDetail, _DetailAlbedoMap);
                OUT.shadowCoord = GetShadowCoord(posInputs);

                // Tangent-space view direction for correct parallax mapping
                float3 bitangentWS = cross(nrmInputs.normalWS, nrmInputs.tangentWS)
                                   * (IN.tangentOS.w * GetOddNegativeScale());
                float3 viewWS = OUT.viewDirWS;
                OUT.viewDirTS = float3(
                    dot(viewWS, nrmInputs.tangentWS),
                    dot(viewWS, bitangentWS),
                    dot(viewWS, nrmInputs.normalWS)
                );

                OUT.vertexLighting = half3(0, 0, 0);
                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < lightCount; ++i)
                    {
                        Light light = GetAdditionalLight(i, posInputs.positionWS);
                        half  NdotL = saturate(dot(nrmInputs.normalWS, light.direction));
                        OUT.vertexLighting += light.color
                            * (pow(light.distanceAttenuation,  _AttenuationPowerrr) * NdotL);
                    }
                #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // ── UV / Parallax (tangent-space — fixes the sliding distortion) ──
                float2 uv = IN.uv;
                float  heightSample = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, uv).r;
                uv += ComputeParallaxOffset(heightSample, _Parallax, IN.viewDirTS);

                // ── Albedo / Detail ──────────────────────────────────────
                half4 col       = SAMPLE_TEXTURE2D(_MainTex,         sampler_MainTex,         uv) * _Color;
                half4 detailCol = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, IN.uvDetail);
                half  detailMask = luminance(detailCol.rgb) * detailCol.a * _DetailProminence;
                half3 albedo     = lerp(col.rgb, detailCol.rgb * _DetailColor.rgb, detailMask);

                clip(col.a - _AlphaClip);

                if (_UseSlice == 1)
                {
                    clip(dot(_SliceNormalOne, IN.positionWS - _SliceCenterOne));
                    clip(dot(_SliceNormalTwo, IN.positionWS - _SliceCenterTwo));
                }

                // ── PBR inputs ───────────────────────────────────────────
                half  roughness = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv).r * _Glossiness;
                half  metallic  = 0;
                half  occlusion = 1;
                half3 emission  = 0;
                half3 normalWS  = normalize(IN.normalWS);

                if (_ColorOnly == 0)
                {
                    metallic  = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv).r * _Metallic;
                    occlusion = SAMPLE_TEXTURE2D(_OcclusionMap,     sampler_OcclusionMap,     uv).r;
                    emission  = SAMPLE_TEXTURE2D(_EmissionMap,      sampler_EmissionMap,      uv).rgb
                                * _EmissionColor.rgb / _EmissionDivision
                                + albedo * IN.vertexLighting;

                    half3 normalTS     = UnpackNormalScale(
                        SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
                    float3 bitangentWS = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                    float3x3 TBN       = float3x3(IN.tangentWS.xyz, bitangentWS, IN.normalWS);
                    normalWS           = normalize(mul(normalTS, TBN));
                }

                // ── Derived surface values ────────────────────────────────
                half  oneMinusReflectivity = 1.0 - lerp(0.04, 1.0, metallic);
                half3 specColor            = lerp(half3(0.04, 0.04, 0.04), albedo, metallic);
                half3 diffColor            = albedo * oneMinusReflectivity;
                half  smoothness           = 1.0 - roughness;
                half  perceptualRoughness  = roughness;

                float a  = roughness * roughness;
                float a2 = a * a;

                // ── GI ───────────────────────────────────────────────────
                half3 giDiffuse  = SampleSH(normalWS) * occlusion;
                half3 reflDir    = reflect(-normalize(IN.viewDirWS), normalWS);
                half3 giSpecular = GlossyEnvironmentReflection(reflDir, perceptualRoughness, occlusion);

                // ── Main light ───────────────────────────────────────────
                Light  mainLight  = GetMainLight(IN.shadowCoord);
                half3  lightColor = mainLight.color
                                  * pow(mainLight.distanceAttenuation, _AttenuationPowerrr)
                                  * mainLight.shadowAttenuation;

                float3 L = normalize(mainLight.direction);
                float3 V = normalize(IN.viewDirWS);
                float3 H = normalize(V + L);

                half preNdotL = saturate(dot(normalWS, L));
                half NdotL    = smoothstep(0, _RampSmoothness, preNdotL);
                half NdotH    = saturate(dot(normalWS, H));
                half NdotV    = abs(dot(normalWS, V));
                half LdotH    = saturate(dot(L, H));

                // Disney diffuse — matches BRP: NdotL in Fresnel terms, single NdotL multiply
                half fd90        = 0.5 + 2.0 * LdotH * LdotH * perceptualRoughness;
                half diffuseTerm = (1.0 + (fd90 - 1.0) * pow(1.0 - NdotL, 5.0))
                                 * (1.0 + (fd90 - 1.0) * pow(1.0 - NdotV, 5.0))
                                 * NdotL;

                // ── Specular ─────────────────────────────────────────────
                float specularTerm = 0;
                half  steps        = 0;

                #ifdef _SPECULARMODE_TRUEPBR
                    steps        = lerp(8, 128, _RampSmoothness);
                    float rNdotH = round(NdotH * steps) / steps;
                    float d      = (rNdotH * a2 - rNdotH) * rNdotH + 1.0;
                    float D      = a2 / (PI * d * d + 1e-7);
                    float lambdaV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
                    float lambdaL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);
                    specularTerm  = (0.5 / (lambdaV + lambdaL + 1e-5)) * D * PI;
                #elif _SPECULARMODE_STYLIZEDPBR
                    steps        = _RampSmoothness * _RampSmoothness * 300;
                    specularTerm = round(pow(NdotH * NdotL, smoothness * 10) * steps) / steps;
                #endif

                specularTerm  = max(0, specularTerm * _SpecularStrength * NdotL);
                specularTerm *= any(specColor) ? 1.0 : 0.0;

                half  surfaceReduction = 1.0 / (roughness * roughness + 1.0);
                half3 fresnelTerm      = specColor + (1.0 - specColor) * pow(1.0 - LdotH, 5.0);
                half  grazingTerm      = saturate(smoothness + (1.0 - oneMinusReflectivity));
                half3 fresnelLerp      = lerp(specColor, grazingTerm, pow(1.0 - NdotV, 5.0));

                half3 color = diffColor * (giDiffuse + lightColor * diffuseTerm)
                            + specularTerm * lightColor * fresnelTerm
                            + surfaceReduction * giSpecular * fresnelLerp;

                // ── Additional pixel lights ───────────────────────────────
                #ifdef _ADDITIONAL_LIGHTS
                    uint addCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < addCount; ++i)
                    {
                        Light  al  = GetAdditionalLight(i, IN.positionWS, half4(1,1,1,1));
                        half3  aLC = al.color
                                   * pow(al.distanceAttenuation, _AttenuationPowerrr)
                                   * al.shadowAttenuation;

                        float3 aL      = normalize(al.direction);
                        float3 aH      = normalize(V + aL);
                        half aPreNdotL = saturate(dot(normalWS, aL));
                        half aNdotL    = smoothstep(0, _RampSmoothness, aPreNdotL);
                        half aNdotH    = saturate(dot(normalWS, aH));
                        half aLdotH    = saturate(dot(aL, aH));

                        half afd90    = 0.5 + 2.0 * aLdotH * aLdotH * perceptualRoughness;
                        half aDiffuse = (1.0 + (afd90 - 1.0) * pow(1.0 - aNdotL, 5.0))
                                      * (1.0 + (afd90 - 1.0) * pow(1.0 - NdotV,  5.0))
                                      * aNdotL;

                        float aSpecular = 0;
                        #ifdef _SPECULARMODE_TRUEPBR
                            float arNdotH = round(aNdotH * steps) / steps;
                            float ad  = (arNdotH * a2 - arNdotH) * arNdotH + 1.0;
                            float aD  = a2 / (PI * ad * ad + 1e-7);
                            float alV = aNdotL * sqrt((-NdotV  * a2 + NdotV)  * NdotV  + a2);
                            float alL = NdotV  * sqrt((-aNdotL * a2 + aNdotL) * aNdotL + a2);
                            aSpecular = (0.5 / (alV + alL + 1e-5)) * aD * PI;
                        #elif _SPECULARMODE_STYLIZEDPBR
                            aSpecular = round(pow(aNdotH * aNdotL, smoothness * 10) * steps) / steps;
                        #endif

                        aSpecular = max(0, aSpecular * _SpecularStrength * aNdotL);
                        aSpecular *= any(specColor) ? 1.0 : 0.0;
                        half3 aFresnel = specColor + (1.0 - specColor) * pow(1.0 - aLdotH, 5.0);

                        color += diffColor * aLC * aDiffuse + aSpecular * aLC * aFresnel;
                    }
                #endif

                color += emission;
                return half4(color, col.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttribs
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            ShadowVaryings ShadowVert(ShadowAttribs IN)
            {
                ShadowVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                float3 posWS  = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(IN.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - posWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                OUT.positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, normWS, lightDir));
                #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE * OUT.positionCS.w);
                #else
                    OUT.positionCS.z = max(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE * OUT.positionCS.w);
                #endif
                return OUT;
            }
            half4 ShadowFrag(ShadowVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttribs
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DepthVaryings DepthVert(DepthAttribs IN)
            {
                DepthVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
            half4 DepthFrag(DepthVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_instancing

            struct DNAttribs
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct DNVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            DNVaryings DepthNormalsVert(DNAttribs IN)
            {
                DNVaryings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                VertexNormalInputs nrmInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                float2 uv        = TRANSFORM_TEX(IN.uv, _MainTex);
                half3  normalTS  = UnpackNormalScale(
                    SAMPLE_TEXTURE2D_LOD(_BumpMap, sampler_BumpMap, uv, 0), _BumpScale);
                float3 bitangentWS = cross(nrmInputs.normalWS, nrmInputs.tangentWS)
                                   * (IN.tangentOS.w * GetOddNegativeScale());
                float3x3 TBN     = float3x3(nrmInputs.tangentWS, bitangentWS, nrmInputs.normalWS);
                OUT.normalWS     = normalize(mul(normalTS, TBN));
                return OUT;
            }
            half4 DepthNormalsFrag(DNVaryings IN) : SV_Target
            {
                return half4(PackNormalOctRectEncode(
                    TransformWorldToViewDir(IN.normalWS, true)), 0, 0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}