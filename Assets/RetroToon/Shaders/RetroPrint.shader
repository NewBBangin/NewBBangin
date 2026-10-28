// Pixel grid and color-precision stages adapted from Kodrin / URP-PSX (MIT).
// Copyright (c) 2020 Codrin-Mihail. See ../ThirdParty/URPPSX/LICENSE.txt.
// Unity 6 / URP 17 port, ink edges and print treatment: BBang RetroToon.
Shader "Hidden/BBang/Retro Print"
{
    Properties
    {
        _PixelHeight ("Virtual image height", Range(180, 1080)) = 360
        _OutlineColor ("Ink color", Color) = (0.11, 0.075, 0.15, 1)
        _OutlineThickness ("Ink width in virtual pixels", Range(0, 3)) = 0.75
        _DepthThreshold ("Silhouette depth tolerance", Range(0.002, 0.15)) = 0.025
        _NormalThreshold ("Crease tolerance", Range(0.05, 0.95)) = 0.24
        _DitherStrength ("Ordered dither", Range(0, 1)) = 0.35
        _ColorSteps ("Color levels per channel", Range(8, 256)) = 48
        _GrainStrength ("Static paper grain", Range(0, 0.08)) = 0.012
        _Saturation ("Color saturation", Range(0, 1.5)) = 1.03
        _Warmth ("Warm paper tint", Range(0, 1)) = 0.18
        _VignetteStrength ("Corner vignette", Range(0, 0.5)) = 0.1
        _ChromaticOffset ("Color registration in virtual pixels", Range(0, 1)) = 0.1
        _EffectEnabled ("Effect blend", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "Retro Print"
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment FragRetroPrint
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _PixelHeight;
                float _OutlineThickness;
                float _DepthThreshold;
                float _NormalThreshold;
                float _DitherStrength;
                float _ColorSteps;
                float _GrainStrength;
                float _Saturation;
                float _Warmth;
                float _VignetteStrength;
                float _ChromaticOffset;
                float _EffectEnabled;
            CBUFFER_END

            float EyeDepth(float2 uv)
            {
                float rawDepth = SampleSceneDepth(saturate(uv));
                // LinearEyeDepth is perspective-only. Keep Scene view / orthographic
                // cameras usable when this material is reused elsewhere.
                if (unity_OrthoParams.w > 0.5)
                {
                    #if UNITY_REVERSED_Z
                        rawDepth = 1.0 - rawDepth;
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
                }
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            float InkFogVisibility(float eyeDepth)
            {
                // Match URP's fragment fog: eye depth measured from the near plane.
                // When fog is disabled, retain full ink instead of using the fog
                // helper's no-keyword fallback (which returns zero intensity).
                bool fogEnabled = false;
                #if defined(FOG_LINEAR_KEYWORD_DECLARED)
                    fogEnabled = fogEnabled || FOG_LINEAR;
                #endif
                #if defined(FOG_EXP_KEYWORD_DECLARED)
                    fogEnabled = fogEnabled || FOG_EXP;
                #endif
                #if defined(FOG_EXP2_KEYWORD_DECLARED)
                    fogEnabled = fogEnabled || FOG_EXP2;
                #endif
                if (!fogEnabled)
                    return 1.0;

                float nearToFarDepth = max(eyeDepth - _ProjectionParams.y, 0.0);
                float fogFactor = ComputeFogFactorZ0ToFar(nearToFarDepth);
                return saturate(ComputeFogIntensity(fogFactor));
            }

            float InkEdge(float2 uv, float2 pixelSize)
            {
                if (_OutlineThickness <= 0.001)
                    return 0.0;

                float2 dx = float2(pixelSize.x * _OutlineThickness, 0.0);
                float2 dy = float2(0.0, pixelSize.y * _OutlineThickness);
                float centerDepth = EyeDepth(uv);
                float4 neighborDepth = float4(
                    EyeDepth(uv + dx), EyeDepth(uv - dx),
                    EyeDepth(uv + dy), EyeDepth(uv - dy));

                float3 centerNormal = SampleSceneNormals(uv);
                float3 normalR = SampleSceneNormals(saturate(uv + dx));
                float3 normalL = SampleSceneNormals(saturate(uv - dx));
                float3 normalT = SampleSceneNormals(saturate(uv + dy));
                float3 normalB = SampleSceneNormals(saturate(uv - dy));

                // Draw depth silhouettes on the nearer surface only. Raising the
                // tolerance at grazing angles avoids drawing ink over flat floors.
                float grazing = 1.0 - abs(TransformWorldToViewDir(centerNormal).z);
                float tolerance = max(_DepthThreshold, 0.0001) * (1.0 + grazing * grazing * 5.0);
                float4 relativeDepth = max(neighborDepth - centerDepth, 0.0) / max(centerDepth, 0.05);
                float depthJump = max(max(relativeDepth.x, relativeDepth.y), max(relativeDepth.z, relativeDepth.w));
                float silhouette = smoothstep(tolerance, tolerance * 1.5, depthJump);

                // Normal creases require valid nearby geometry on both sides.
                // Sky has a zero normal; it must not become a full-screen ink mask.
                float4 validNeighbors = step(0.25, float4(
                    dot(normalR, normalR), dot(normalL, normalL),
                    dot(normalT, normalT), dot(normalB, normalB)));
                float4 normalDifference = saturate(1.0 - float4(
                    dot(centerNormal, normalR), dot(centerNormal, normalL),
                    dot(centerNormal, normalT), dot(centerNormal, normalB))) * validNeighbors;
                float crease = max(max(normalDifference.x, normalDifference.y), max(normalDifference.z, normalDifference.w));
                float normalThreshold = clamp(_NormalThreshold, 0.01, 0.95);
                crease = smoothstep(normalThreshold, min(normalThreshold + 0.18, 1.0), crease);
                crease *= step(0.25, dot(centerNormal, centerNormal));
                return saturate(max(silhouette, crease * 0.8)) * InkFogVisibility(centerDepth);
            }

            // Adapted from URP-PSX's Get4x4TexValue coordinate indexing.
            // A centered Bayer rank replaces binary cutout, preserving colored ink
            // and fine transparent lettering while quantizing color precision.
            float OrderedDither(float2 pixelCoordinate)
            {
                const float4x4 bayer = float4x4(
                     0,  8,  2, 10,
                    12,  4, 14,  6,
                     3, 11,  1,  9,
                    15,  7, 13,  5);
                uint2 cell = uint2(pixelCoordinate) % 4;
                return (bayer[cell.y][cell.x] + 0.5) / 16.0 - 0.5;
            }

            float PaperGrain(float2 pixelCoordinate)
            {
                float3 p = frac(float3(pixelCoordinate.xyx) * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z) - 0.5;
            }

            half4 FragRetroPrint(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half4 original = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0);
                if (_EffectEnabled <= 0.0001)
                    return original;

                float targetHeight = clamp(_PixelHeight, 1.0, _ScaledScreenParams.y);
                float2 resolution = float2(max(1.0, floor(targetHeight * _ScaledScreenParams.x / _ScaledScreenParams.y)), targetHeight);
                float2 pixelSize = rcp(resolution);
                // Pixelation.shader's floor(uv * resolution) / resolution, with
                // half a texel added so samples land at pixel centers.
                float2 pixelCoordinate = floor(uv * resolution);
                float2 snappedUV = (pixelCoordinate + 0.5) * pixelSize;
                half4 sampleColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, snappedUV, 0);
                float3 color = sampleColor.rgb;

                if (_ChromaticOffset > 0.001)
                {
                    float2 offset = float2(pixelSize.x * _ChromaticOffset, 0.0);
                    color.r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, snappedUV + offset, 0).r;
                    color.b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, snappedUV - offset, 0).b;
                }

                // Quantize perceptual color, keeping saturated mint, red and pink.
                #if !defined(UNITY_COLORSPACE_GAMMA)
                    color = LinearToSRGB(max(color, 0.0));
                #endif
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(luminance.xxx, color, _Saturation);
                color *= lerp(float3(1.0, 1.0, 1.0), float3(1.06, 1.015, 0.94), _Warmth);
                float steps = max(2.0, _ColorSteps);
                float dither = OrderedDither(pixelCoordinate) * _DitherStrength;
                color = floor(saturate(color) * steps + 0.5 + dither) / steps;
                color += PaperGrain(pixelCoordinate) * _GrainStrength;

                float2 centeredUV = uv * 2.0 - 1.0;
                float vignette = smoothstep(0.25, 1.5, dot(centeredUV, centeredUV));
                color *= 1.0 - vignette * _VignetteStrength;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                    color = SRGBToLinear(saturate(color));
                #endif

                float ink = InkEdge(snappedUV, pixelSize) * _OutlineColor.a;
                color = lerp(color, _OutlineColor.rgb, ink);
                return half4(lerp(original.rgb, color, saturate(_EffectEnabled)), sampleColor.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
