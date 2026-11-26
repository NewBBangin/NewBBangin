Shader "BBang/Retro Sunset Sky"
{
    Properties
    {
        _Olive ("Olive haze", Color) = (0.58, 0.60, 0.40, 1)
        _Apricot ("Apricot sunset", Color) = (0.88, 0.49, 0.40, 1)
        _Horizon ("Horizon paper", Color) = (0.63, 0.62, 0.44, 1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Olive, _Apricot, _Horizon;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float sunset = smoothstep(-0.65, 0.8, direction.x);
                half3 upper = lerp(_Olive.rgb, _Apricot.rgb, sunset);
                float height = smoothstep(-0.1, 0.65, direction.y);
                return half4(lerp(_Horizon.rgb, upper, height), 1);
            }
            ENDHLSL
        }
    }
}
