Shader "Custom/CustomClouds"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.6, 0.6, 0.7, 1)
        _Density ("Density", Range(0, 5)) = 1.0
        _Falloff ("Edge Falloff", Range(0, 2)) = 1.0
        _NoiseTex ("3D Noise Texture", 3D) = "" {}
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _Speed ("Noise Scroll Speed", Range(0, 2)) = 0.2
        _LightInfluence ("Light Influence", Range(0, 2)) = 1.0
        _DistanceFadeStart ("Distance Fade Start", Range(0, 200)) = 10.0
        _DistanceFadeEnd ("Distance Fade End", Range(1, 500)) = 50.0
        _AlphaThreshold ("Alpha Cutoff Threshold", Range(0, 1)) = 0.1

        [Toggle]_UseRed ("Use Red Channel", Float) = 1
        [Toggle]_UseGreen ("Use Green Channel", Float) = 0
        [Toggle]_UseBlue ("Use Blue Channel", Float) = 0

        [Toggle]_EnableDepthFade ("Obscure Objects Under Fog", Float) = 0
        _DepthFadeStrength ("Obscure Strength", Range(0, 2)) = 1.0
        _DepthFadeRange ("Obscure Distance Range", Range(0.1, 50)) = 5.0
        _FogBlendColor ("Fog Blend Color", Color) = (0.5, 0.5, 0.6, 1)

        [Toggle]_DebugDepthView ("Debug Depth Texture", Float) = 0
        _DebugDepthScale ("Debug Depth Scale", Range(0.01, 5)) = 1.0
    }

    SubShader
    {
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }
      


        Pass
        {
            Name "VolumetricFog"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
            };

            float4 _BaseColor;
            float _Density;
            float _Falloff;
            float _NoiseScale;
            float _Speed;
            float _LightInfluence;
            float _DistanceFadeStart;
            float _DistanceFadeEnd;
            float _AlphaThreshold;
            float _UseRed;
            float _UseGreen;
            float _UseBlue;

            float _EnableDepthFade;
            float _DepthFadeStrength;
            float _DepthFadeRange;
            float4 _FogBlendColor;

            float _DebugDepthView;
            float _DebugDepthScale;

            TEXTURE3D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // --- DEBUG DEPTH VIEW (moved to top for early return) ---
                if (_DebugDepthView > 0.5)
                {
                    float2 screenUV = IN.positionCS.xy / _ScreenParams.xy;

                    // Sample scene depth using URP's built-in function
                    float sceneDepth = SampleSceneDepth(screenUV);
                    
                    // Linearize the depth for visualization
                    float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                    
                    // Scale for better visualization (adjust _DebugDepthScale in inspector)
                    float visualDepth = saturate(linearDepth * _DebugDepthScale * 0.01);
                    
                    return half4(visualDepth.xxx, 1);
                }

                // Edge fade (keep fog inside cube)
                float3 absPos = abs(IN.positionOS);
                float edgeFade = 1.0 - smoothstep(0.8, 1.0, max(absPos.x, max(absPos.y, absPos.z)));

                // Noise lookup
                float3 noiseUV = IN.positionWS * _NoiseScale * 0.01;
                noiseUV += float3(_Time.y * _Speed, _Time.y * _Speed * 0.7, _Time.y * _Speed * 0.5);
                float3 noiseSample = SAMPLE_TEXTURE3D(_NoiseTex, sampler_NoiseTex, frac(noiseUV)).rgb;

                float total = _UseRed + _UseGreen + _UseBlue;
                total = max(total, 1.0);
                float noiseVal = (noiseSample.r * _UseRed +
                                  noiseSample.g * _UseGreen +
                                  noiseSample.b * _UseBlue) / total;

                // Density
                float density = saturate(noiseVal * _Density * edgeFade);

                // Distance fade
                float3 camPos = GetCameraPositionWS();
                float dist = distance(IN.positionWS, camPos);
                float camFade = saturate(1.0 - smoothstep(_DistanceFadeStart, _DistanceFadeEnd, dist));
                density *= camFade;

                // Lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                float3 lightColor = mainLight.color * NdotL * _LightInfluence + 0.2;

                float3 finalColor = _BaseColor.rgb * lightColor;
                float alpha = density * _BaseColor.a;

                // Depth-based obscuring logic
                if (_EnableDepthFade > 0.5)
                {
                    // Calculate screen UV correctly
                    float2 screenUV = IN.positionCS.xy / _ScreenParams.xy;

                    // Sample depth behind fog
                    float sceneDepth = SampleSceneDepth(screenUV);
                    float sceneLinearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                    float fogLinearDepth = LinearEyeDepth(IN.positionCS.z / IN.positionCS.w, _ZBufferParams);

                    // Calculate how far the background object is behind the fog
                    float depthBehindFog = max(0, sceneLinearDepth - fogLinearDepth);
                    
                    // Convert to obscuring factor (0 = no obscuring, 1 = fully obscured)
                    float obscureFactor = saturate(depthBehindFog / _DepthFadeRange) * _DepthFadeStrength;
                    
                    // Blend fog color over the existing color based on obscure factor
                    // This simulates fog accumulation - deeper objects get more fog color
                    finalColor = lerp(finalColor, _FogBlendColor.rgb * lightColor, obscureFactor);
                    
                    // Also increase alpha to make fog more opaque where objects are behind it
                    alpha = saturate(alpha + obscureFactor * density);
                }

                // Threshold
                if (alpha < _AlphaThreshold)
                    alpha = 0;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}