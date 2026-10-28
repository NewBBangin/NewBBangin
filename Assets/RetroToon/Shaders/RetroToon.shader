// BBang retro cartoon surface, adapted from the MIT UnityURPToonLitShaderExample.
// See ../ThirdParty/UnityURPToonLitShaderExample/LICENSE.txt and SOURCE.txt.
Shader "BBang/Retro Toon"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.78, 0.64, 0.43, 1)
        _TextureStrength ("Texture Strength", Range(0, 1)) = 1
        _ShadowColor ("Shadow Tint", Color) = (0.50, 0.38, 0.56, 1)
        _ShadowThreshold ("Cel Shadow Threshold", Range(-1, 1)) = 0.15
        _ShadowSoftness ("Cel Edge Softness", Range(0.001, 0.25)) = 0.012
        _ShadowStrength ("Shadow Tint Strength", Range(0, 1)) = 0.75
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "UniversalMaterialType" = "SimpleLit"
        }
        LOD 200
        Cull Back

        HLSLINCLUDE
        #include "RetroToonInput.hlsl"
        ENDHLSL

        Pass
        {
            Name "RetroToonForward"
            Tags { "LightMode" = "UniversalForwardOnly" }
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex RetroToonVertex
            #pragma fragment RetroToonFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include "RetroToonForward.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode" = "DepthNormalsOnly" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
