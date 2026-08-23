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
        _MaxOutlineDistance("Max Outline Distance", Range(1, 100)) = 25
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
            float _MaxOutlineDistance;


            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag
            
            
            // Helper function to sample scene normals remapped from [-1, 1] range to [0, 1].
            float3 SampleSceneNormalsRemapped(float2 uv)
            {
                return SampleSceneNormals(uv) * 0.5 + 0.5;
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
                
                float rawCenterDepth = SampleSceneDepth(IN.texcoord);
                if (rawCenterDepth >= 0.99999)
                    return originalColor;

                float centerLinearDepth = LinearEyeDepth(rawCenterDepth, _ZBufferParams);
                
                float depthScale = saturate(_MaxOutlineDistance / centerLinearDepth); // Distance-based thickness attenuation
                // Built-in texel size avoids manual per-pixel division
                float2 texelSize = _OutlineThickness * depthScale * _BlitTexture_TexelSize.xy;
                
                // Roberts Cross 4-tap sampling coordinates
                float2 uv0 = IN.texcoord + float2(-texelSize.x, -texelSize.y);
                float2 uv1 = IN.texcoord + float2( texelSize.x,  texelSize.y);
                float2 uv2 = IN.texcoord + float2( texelSize.x, -texelSize.y);
                float2 uv3 = IN.texcoord + float2(-texelSize.x,  texelSize.y);

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
                combinedEdge = smoothstep(_EdgeThreshold, _EdgeThreshold + 0.05, combinedEdge);
                
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