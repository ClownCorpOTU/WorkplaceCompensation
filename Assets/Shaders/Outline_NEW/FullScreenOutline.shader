Shader "Outline/FullScreenOutline"
{
    Properties
    {
        _SilhouetteColor("Silhouette Color", Color) = (0, 0, 0, 1)
        _InnerCreaseColor("Inner Crease Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Range(0, 10)) = 1
        
        _DepthSensitivity("Depth Sensitivity", Range(0, 50)) = 10
        _NormalSensitivity("Normal Sensitivity", Range(0, 10)) = 1
        _LuminanceSensitivity("Luminance Sensitivity", Range(0, 10)) = 1
        
        _EdgeThreshold("Edge Threshold", Range(0, 1)) = 0.1
        _EdgeSoftness("Edge Softness", Range(0.01, 0.5)) = 0.15
        _MaxOutlineDistance("Max Outline Distance", Range(1, 100)) = 25
        
        _WiggleFrequency("Wiggle Frequency", Range(10, 500)) = 150
        _WiggleStrength("Wiggle Strength", Range(0, 5)) = 1.0
        _ThicknessVariation("Thickness Variation", Range(0, 1)) = 0.3
        _LineBoilFPS("Line Boil FPS", Range(0, 24)) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass 
        {
            Name "EDGE DETECTION OUTLINE"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl" // needed to sample scene depth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" // needed to sample scene normals
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // needed to sample scene color/luminance

            float4 _SilhouetteColor;
            float4 _InnerCreaseColor;
            float _OutlineThickness;
            float _DepthSensitivity;
            float _NormalSensitivity;
            float _LuminanceSensitivity;
            float _EdgeThreshold;
            float _EdgeSoftness;
            float _MaxOutlineDistance;
            float _WiggleFrequency;
            float _WiggleStrength;
            float _ThicknessVariation;
            float _LineBoilFPS;


            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag
            
            
            // Helper function to sample scene normals remapped from [-1, 1] range to [0, 1].
            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv);
                //return SampleSceneNormals(uv) * 0.5 + 0.5;
            }

            // Helper function to sample scene luminance.
            float SampleSceneLuminance(float2 uv)
            {
                float3 color = SampleSceneColor(uv);
                return color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
            }

            
            // Sample 4 diagonal offsets (Roberts Cross) instead of 9-tap Sobel
            half4 frag(Varyings IN) : SV_TARGET
            {
                float4 originalColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                
                // Finding center depth for a gradual fade
                float rawCenterDepth = SampleSceneDepth(IN.texcoord);
                if (rawCenterDepth >= 0.99999)
                    return originalColor;

                float centerLinearDepth = LinearEyeDepth(rawCenterDepth, _ZBufferParams);
                
                // 1. Calculate base texel size, scaled by distance so outlines fade out
                float depthScale = saturate(_MaxOutlineDistance / centerLinearDepth);
                float2 baseTexelSize = _OutlineThickness * depthScale * _BlitTexture_TexelSize.xy;
                
                // 2. Quantize time into discrete animation steps
                float steppedTime = (_LineBoilFPS > 0.0) ? floor(_Time.y * _LineBoilFPS) : 0.0;
                
                // 3. Derive object-specifc phase seed so outlines vary based on depth and normals
                float3 centerNormal = SampleSceneNormalsRemapped(IN.texcoord);
                // Generate an object-specific phase seed, combining facing angle and depth
                float surfaceSeed = dot(centerNormal, float3(12.9898, 78.233, 45.164)) * 10.0 + (centerLinearDepth * 0.5);
                
                // 4. Compute dynamic pen pressure along surface coordinates
                // Offset frequence slightly so width changes don't peak at the exact same spot as the spatial wobble
                // Scalar multiplier decouples thickness popping from the position wobble
                float pressureWave = sin((IN.texcoord.x + IN.texcoord.y) * (_WiggleFrequency * 0.75) + surfaceSeed + (steppedTime * 1.3));
                float penPressure = max(0.05, 1.0 + (_ThicknessVariation * pressureWave));
                
                // Apply pen pressure to sample radius
                float2 texelSize = baseTexelSize * penPressure;
                
                // 5. Finally, calculate spatial jitter with decorrelated X/Y frame stepping
                float jitterAtten = depthScale;
                float2 jitter = float2(
                    sin(IN.texcoord.y * _WiggleFrequency + surfaceSeed + (steppedTime * 1.7)),
                    cos(IN.texcoord.x * _WiggleFrequency + surfaceSeed + (steppedTime * 2.3))
                ) * (_WiggleStrength * jitterAtten * texelSize);
                
                // Offset sampling center
                float2 centerUV = IN.texcoord + jitter;
                
                // Roberts Cross 4-tap sampling coordinates
                float2 uv0 = centerUV + float2(-texelSize.x, -texelSize.y);
                float2 uv1 = centerUV + float2( texelSize.x,  texelSize.y);
                float2 uv2 = centerUV + float2( texelSize.x, -texelSize.y);
                float2 uv3 = centerUV + float2(-texelSize.x,  texelSize.y);

                float depthEdge = 0.0;                
                if (_DepthSensitivity > 0.0)
                {
                    float d0 = LinearEyeDepth(SampleSceneDepth(uv0), _ZBufferParams);
                    float d1 = LinearEyeDepth(SampleSceneDepth(uv1), _ZBufferParams);
                    float d2 = LinearEyeDepth(SampleSceneDepth(uv2), _ZBufferParams);
                    float d3 = LinearEyeDepth(SampleSceneDepth(uv3), _ZBufferParams);

                    // Scale by centerLinearDepth to eliminate flat surface perspective slope artifacts
                    float deltaDepth1 = (d1 - d0) / centerLinearDepth;
                    float deltaDepth2 = (d3 - d2) / centerLinearDepth;
                    depthEdge = (abs(deltaDepth1) + abs(deltaDepth2)) * _DepthSensitivity;
                    
                    // Reconstruct world position from depth
                    float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, rawCenterDepth, UNITY_MATRIX_I_VP);
                    float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                    
                    // Calculate grazing angle factor (1 when facing camera, 0 when viewing edge on)
                    float NdotV = saturate(dot(centerNormal, viewDir));
                    // Smoothstep aggressively cuts off depth edges on shallow surfaces
                    depthEdge *= smoothstep(0.15, 0.5, NdotV);
                }

                float normalEdge = 0.0;
                if (_NormalSensitivity > 0.0)
                {
                    float3 n0 = SampleSceneNormalsRemapped(uv0);
                    float3 n1 = SampleSceneNormalsRemapped(uv1);
                    float3 n2 = SampleSceneNormalsRemapped(uv2);
                    float3 n3 = SampleSceneNormalsRemapped(uv3);

                    float3 deltaNorm1 = n1 - n0;
                    float3 deltaNorm2 = n3 - n2;
                    normalEdge = (dot(deltaNorm1, deltaNorm1) + dot(deltaNorm2, deltaNorm2)) * _NormalSensitivity;
                }

                float luminanceEdge = 0.0;
                if (_LuminanceSensitivity > 0.0)
                {
                    float l0 = SampleSceneLuminance(uv0);
                    float l1 = SampleSceneLuminance(uv1);
                    float l2 = SampleSceneLuminance(uv2);
                    float l3 = SampleSceneLuminance(uv3);

                    luminanceEdge = (abs(l1 - l0) + abs(l3 - l2)) * _LuminanceSensitivity;
                }

                float combinedEdge = max(depthEdge, max(normalEdge, luminanceEdge));
                combinedEdge = smoothstep(_EdgeThreshold - _EdgeSoftness, _EdgeThreshold + _EdgeSoftness, combinedEdge);
                
                float3 inkedSilhoutteColor = originalColor.rgb * _SilhouetteColor.rgb;
                float3 inkedInnerColor = originalColor.rgb * _InnerCreaseColor.rgb;
                
                float3 outline = lerp(inkedInnerColor, inkedSilhoutteColor, step(normalEdge, depthEdge));
                float3 finalColor = lerp(originalColor.rgb, outline, saturate(combinedEdge));
                
                return float4(finalColor, originalColor.a);
            }
            ENDHLSL
        }
    }
}