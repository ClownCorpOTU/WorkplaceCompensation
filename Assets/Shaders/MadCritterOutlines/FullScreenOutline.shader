Shader "Outline/FullScreenOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Range(0, 10)) = 1
        _DepthSensitivity("Depth Sensitivity", Range(0, 50)) = 10
        _NormalSensitivity("Normal Sensitivity", Range(0, 10)) = 1
        _LuminanceSensitivity("Luminance Sensitivity", Range(0, 10)) = 1
        _EdgeThreshold("Edge Threshold", Range(0, 1)) = 0.1
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

            float4 _OutlineColor;
            float _OutlineThickness;
            float _DepthSensitivity;
            float _NormalSensitivity;
            float _LuminanceSensitivity;
            float _EdgeThreshold;


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
            
            float DetectDepthEdge(float2 uv, float2 texelSize)
            {
                float sobelX = 0.0;
                float sobelY = 0.0;
                
                float sobelXWeights[9] = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                float sobelYWeights[9] = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
                
                int index = 0;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float depth = LinearEyeDepth(SampleSceneDepth(uv + offset), _ZBufferParams);
                        sobelX += depth * sobelXWeights[index];
                        sobelY += depth * sobelYWeights[index];
                        index++;
                    }
                }
                return sqrt(sobelX * sobelX + sobelY * sobelY);
            }
            
            float DetectNormalEdge(float2 uv, float2 texelSize)
            {
                float3 normalL = SampleSceneNormalsRemapped(uv + float2(-texelSize.x, 0));
                float3 normalR = SampleSceneNormalsRemapped(uv + float2(texelSize.x, 0));
                float3 normalU = SampleSceneNormalsRemapped(uv + float2(0, texelSize.y));
                float3 normalD = SampleSceneNormalsRemapped(uv + float2(0, -texelSize.y));
                
                float edgeX = length(normalR - normalL);
                float edgeY = length(normalU - normalD);
                return (edgeX + edgeY) * 0.5;
            }
            
            float DetectLuminanceEdge(float2 uv, float2 texelSize)
            {
                float sobelX = 0.0;
                float sobelY = 0.0;
                
                float sobelXWeights[9] = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                float sobelYWeights[9] = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
                
                int index = 0;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float depth = SampleSceneLuminance(uv + offset);
                        sobelX += depth * sobelXWeights[index];
                        sobelY += depth * sobelYWeights[index];
                        index++;
                    }
                }
                return sqrt(sobelX * sobelX + sobelY * sobelY);
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float4 originalColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float2 texelSize = _OutlineThickness * float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                
                float centerDepth = SampleSceneDepth(IN.texcoord);
                if (centerDepth >= 0.99999)
                    return originalColor;
                
                float depthEdge = DetectDepthEdge(IN.texcoord, texelSize) * _DepthSensitivity;
                float normalEdge = DetectNormalEdge(IN.texcoord, texelSize) * _NormalSensitivity;
                float luminanceEdge = DetectLuminanceEdge(IN.texcoord, texelSize) * _LuminanceSensitivity;
                float combinedEdge = max(depthEdge, normalEdge);
                combinedEdge = max(combinedEdge, luminanceEdge);
                
                combinedEdge = smoothstep(_EdgeThreshold, _EdgeThreshold + 0.05, combinedEdge);
                combinedEdge = saturate(combinedEdge * 2.0);
                
                float3 finalColor = lerp(originalColor.rgb, _OutlineColor.rgb, combinedEdge);
                return float4(finalColor, originalColor.a);
            }
            ENDHLSL
        }
    }
}