Shader "Custom/CustomFog"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.6, 0.6, 0.7, 1) // Colour of the fog
        _Density ("Density", Range(0, 5)) = 1.0 // Overall density multiplier
        _Falloff ("Edge Falloff", Range(0, 2)) = 1.0 // How quickly the fog fades out at the edges
        _NoiseTex ("3D Noise Texture", 3D) = "" {} // 3D noise texture for volumetric effect
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0 // Scale of the noise texture
        _Speed ("Noise Scroll Speed", Range(0, 2)) = 0.2 // Speed of noise animation
        _LightInfluence ("Light Influence", Range(0, 2)) = 1.0 // How much the main light affects the fog
        _DistanceFadeStart ("Distance Fade Start", Range(0, 200)) = 10.0 // Distance from camera where fog starts to fade
        _DistanceFadeEnd ("Distance Fade End", Range(1, 500)) = 50.0 // Distance from camera where fog is fully faded
        _AlphaThreshold ("Alpha Cutoff Threshold", Range(0, 1)) = 0.1 // Minimum alpha for rendering
        _DepthFadeDistance ("Depth Fade Distance", Range(0, 10)) = 2.0 // Distance over which to fade based on depth difference, hides individual layers quite well.

        [Header(Depth Obscure)]
        [Toggle(_DEPTH_OBSCURE)] _UseDepthObscure ("Enable Depth Obscure", Float) = 0 // Toggle for depth obscure feature
        _DepthObscureMin ("Depth Obscure Min", Range(0, 100)) = 5.0 // Minimum depth for depth obscure effect
        _DepthObscureMax ("Depth Obscure Max", Range(0, 200)) = 50.0 // Maximum depth for depth obscure effect
        _DepthObscureStrength ("Depth Obscure Strength", Range(0, 5)) = 2.0 // Strength of depth obscure effect

        [Toggle]_UseRed ("Use Red Channel", Float) = 1
        [Toggle]_UseGreen ("Use Green Channel", Float) = 0
        [Toggle]_UseBlue ("Use Blue Channel", Float) = 0
    }

    SubShader
    {
        //does not write to depth buffer and is transparent
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
            #pragma shader_feature _DEPTH_OBSCURE
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
                float4 screenPos  : TEXCOORD3;
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
            float _DepthFadeDistance;
            float _UseRed;
            float _UseGreen;
            float _UseBlue;

            #ifdef _DEPTH_OBSCURE
            float _DepthObscureMin;
            float _DepthObscureMax;
            float _DepthObscureStrength;
            #endif

            TEXTURE3D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Depth calculations
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float fragDepth = LinearEyeDepth(IN.positionCS.z, _ZBufferParams);
                float depthDiff = sceneDepth - fragDepth;

                // Calculate depth fade (soft particles) - always needed. 
                float depthFade = saturate(depthDiff / _DepthFadeDistance);

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

                // Base density with depth fade applied
                float density = saturate(noiseVal * _Density * edgeFade * depthFade);

                #ifdef _DEPTH_OBSCURE
                // Add depth-based density increase for distant objects
                // Only apply to geometry behind the fog (positive depthDiff)
                if (depthDiff > 0.0)
                {
                    // Map scene depth to 0-1 range between min and max with smoothstep for gradual transition
                    float normalizedDepth = saturate((sceneDepth - _DepthObscureMin) / max(0.01, _DepthObscureMax - _DepthObscureMin));
                    float depthDensity = smoothstep(0.0, 1.0, normalizedDepth) * _DepthObscureStrength;
                    
                    // Add depth density on top of existing density (which already has depthFade applied)
                    // This way soft particles still work, but we add more fog for distant objects
                    density = saturate(density + depthDensity * depthFade);
                }
                #endif

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