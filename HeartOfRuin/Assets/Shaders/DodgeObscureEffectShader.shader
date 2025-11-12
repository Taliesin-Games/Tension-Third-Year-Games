Shader "Custom/DodgeObscureEffectShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 1
        _Softness ("Edge Softness", Range(0,1)) = 0.3
        _HeightFade ("Height Fade", Range(0,1)) = 0.5
        _AlphaMapIntensity ("Alpha Map Influence", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_AlphaMap);
            SAMPLER(sampler_AlphaMap);
            float4 _AlphaMap_ST;

            float4 _TintColor;
            float _Alpha;
            float _Softness;
            float _HeightFade;
            float _AlphaMapIntensity;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alphaMap = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, input.uv).r;

                // Combine color and tint
                half4 finalCol = baseCol * _TintColor;


                half combinedAlpha = _Alpha * lerp(1.0, alphaMap, _AlphaMapIntensity);

                // Height fade (bottom to top)
                float heightFactor = saturate(input.positionWS.y * _HeightFade);
                combinedAlpha *= (1.0 - heightFactor);
                

                // Edge fade (soften around sides)
                float edgeFade = smoothstep(0.5 - _Softness, 0.5, abs(input.uv.x - 0.5));
                combinedAlpha *= (1.0 - edgeFade);

                finalCol.a = combinedAlpha;
                return finalCol;
            }
            ENDHLSL
        }
    }

    FallBack Off
}

