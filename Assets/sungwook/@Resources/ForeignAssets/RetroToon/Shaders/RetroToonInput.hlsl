// Based on ColinLeung-NiloCat/UnityURPToonLitShaderExample (MIT).
// Pinned source and license: ../ThirdParty/UnityURPToonLitShaderExample/.
#ifndef BBANG_RETRO_TOON_INPUT_INCLUDED
#define BBANG_RETRO_TOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// Every pass includes exactly this layout, preserving SRP Batcher compatibility.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _ShadowColor;
    half4 _EmissionColor;
    half _ShadowThreshold;
    half _ShadowSoftness;
    half _ShadowStrength;
    half _TextureStrength;
CBUFFER_END

#endif
