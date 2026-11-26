// Adapted from ShadeSingleLight / CompositeAllLightResults in the MIT example:
// https://github.com/ColinLeung-NiloCat/UnityURPToonLitShaderExample
// Original copyright (c) 2020 ColinLeung-NiloCat. See bundled LICENSE.txt.
#ifndef BBANG_RETRO_TOON_FORWARD_INCLUDED
#define BBANG_RETRO_TOON_FORWARD_INCLUDED

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    half fogFactor : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings RetroToonVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    return output;
}

half4 RetroToonFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half3 textureColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
    half3 albedo = _BaseColor.rgb * lerp(half3(1, 1, 1), textureColor, _TextureStrength);
    half3 normalWS = NormalizeNormalPerPixel(input.normalWS);

    // Evaluate cascades per pixel so large floor polygons do not show seams.
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
        float4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(input.positionWS));
    #else
        float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    #endif
    Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));

    // The example's single smoothstep replaces the continuous Lambert gradient.
    half softness = max(_ShadowSoftness, 0.0001h);
    half litOrShadowArea = smoothstep(_ShadowThreshold - softness,
                                     _ShadowThreshold + softness,
                                     dot(normalWS, mainLight.direction));
    litOrShadowArea *= mainLight.shadowAttenuation;
    half3 shadowTint = lerp(half3(1, 1, 1), _ShadowColor.rgb, _ShadowStrength);
    half3 lightBand = lerp(shadowTint, half3(1, 1, 1), litOrShadowArea);

    // Constant ambient avoids rounded PBR shading and preserves the graphic palette.
    half3 ambient = max(SampleSH(half3(0, 0, 0)), half3(0.12h, 0.12h, 0.12h));
    half3 directLight = saturate(mainLight.color) * lightBand * mainLight.distanceAttenuation;
    half3 color = albedo * max(ambient, directLight) + _EmissionColor.rgb;
    color = MixFog(color, input.fogFactor);
    return half4(color, 1);
}

#endif
