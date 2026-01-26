Shader "Hidden/Edge Detection"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        
        // Added for camera depth
        _DepthFallOff ("Depth Falloff", Float) = 0.1
        _MinThickness ("Minimum Thickness", Float) = 0.5
        
        // Making it look more hand-drawn
        _WobbleStrength ("Wobble Strength", Float) = 0.002
        _WobbleFrequency ("Wobble Frequency", Float) = 10.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            Name "EDGE DETECTION OUTLINE"
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl" // needed to sample scene depth
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" // needed to sample scene normals
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" // needed to sample scene color/luminance

            float _OutlineThickness;
            float4 _OutlineColor;
            
            // Camera depth
            float _DepthFalloff;
            float _MinThickness;

            // Hand-drawn
            float _WobbleStrength;
            float _WobbleFrequency;

            #pragma vertex Vert // vertex shader is provided by the Blit.hlsl include
            #pragma fragment frag

            // Edge detection kernel that works by taking the sum of the squares of the differences between diagonally adjacent pixels (Roberts Cross).
            float RobertsCross(float3 samples[4])
            {
                const float3 difference_1 = samples[1] - samples[2];
                const float3 difference_2 = samples[0] - samples[3];
                return sqrt(dot(difference_1, difference_1) + dot(difference_2, difference_2));
            }

            // The same kernel logic as above, but for a single-value instead of a vector3.
            float RobertsCross(float samples[4])
            {
                const float difference_1 = samples[1] - samples[2];
                const float difference_2 = samples[0] - samples[3];
                return sqrt(difference_1 * difference_1 + difference_2 * difference_2);
            }
            
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

            // Smooth, wavy distortion
            float2 ApplyWobble(float2 uv)
            {
                // We use sin and cos to create a circular "swirl" or wave effect.
                // Using uv.y to influence x, and uv.x to influence y ensures the lines 
                // don't just shift in one direction.
                float2 wobble;
                wobble.x = sin(uv.y * _WobbleFrequency) * _WobbleStrength;
                wobble.y = cos(uv.x * _WobbleFrequency) * _WobbleStrength;
                
                return uv + wobble;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                // Screen-space coordinates which we will use to sample.
                float2 uv = IN.texcoord;
                float2 texel_size = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                // 1. Get linear depth
                uv = ApplyWobble(uv);
                float rawDepth = SampleSceneDepth(uv);
                float linear01Depth = Linear01Depth(rawDepth, _ZBufferParams);

                // 2. Modified scaling math
                // (1-depth) decreases thickness as depth increases. _DepthFallOff is a power to control the curve
                float distanceMultiplier = pow(1.0 - linear01Depth, _DepthFalloff * 100.0);
                float scaledThickness = _OutlineThickness * distanceMultiplier;
                scaledThickness = max(scaledThickness, _MinThickness);

                // 3. Apply to UV offsets
                float2 uvs[4];
                float half_w = scaledThickness * 0.5;
                
                uvs[0] = uv + texel_size * float2(-half_w,  half_w);
                uvs[1] = uv + texel_size * float2( half_w,  half_w);
                uvs[2] = uv + texel_size * float2(-half_w, -half_w);
                uvs[3] = uv + texel_size * float2( half_w, -half_w);

                /*
                // Generate 4 diagonally placed samples.
                const float half_width_f = floor(scaledThickness * 0.5);
                //const float half_width_f = floor(_OutlineThickness * 0.5);
                const float half_width_c = ceil(scaledThickness * 0.5);
                //const float half_width_c = ceil(_OutlineThickness * 0.5);

                float2 uvs[4];
                uvs[0] = uv + texel_size * float2(half_width_f, half_width_c) * float2(-1, 1);  // top left
                uvs[1] = uv + texel_size * float2(half_width_c, half_width_c) * float2(1, 1);   // top right
                uvs[2] = uv + texel_size * float2(half_width_f, half_width_f) * float2(-1, -1); // bottom left
                uvs[3] = uv + texel_size * float2(half_width_c, half_width_f) * float2(1, -1);  // bottom right
                */
                
                float3 normal_samples[4];
                float depth_samples[4], luminance_samples[4];
                
                for (int i = 0; i < 4; i++) {
                    depth_samples[i] = SampleSceneDepth(uvs[i]);
                    normal_samples[i] = SampleSceneNormalsRemapped(uvs[i]);
                    luminance_samples[i] = SampleSceneLuminance(uvs[i]);
                }

                // --- 1. Calculate the Raw Edge Strengtsh ---
                float raw_depth = RobertsCross(depth_samples);
                float raw_normal = RobertsCross(normal_samples);
                float raw_luminance = RobertsCross(luminance_samples);

                // --- 2. "Soft" Thresholding ---
                // This allows the edge to have a soft "fade-in" instead of disappearing
                float edge_depth = smoothstep(0.001, 0.01, raw_depth);
                float edge_normal = smoothstep(0.1, 0.5, raw_normal);
                float edge_luminance = smoothstep(0.1, 0.3, raw_luminance);

                // --- 3. Combine them ---
                float edge = max(edge_depth, max(edge_normal, edge_luminance));

                // --- 4. Distance/Thickness polish ---
                // We make the line 50% transparent if it's thickness is 0.5
                float alphaMultiplier = saturate(scaledThickness);
                float finalEdge = edge * alphaMultiplier;

                return finalEdge * _OutlineColor;

                /*
                // Apply edge detection kernel on the samples to compute edges.
                float edge_depth = RobertsCross(depth_samples);
                float edge_normal = RobertsCross(normal_samples);
                float edge_luminance = RobertsCross(luminance_samples);
                
                // Threshold the edges (discontinuity must be above certain threshold to be counted as an edge). The sensitivities are hardcoded here.
                float depth_threshold = 1 / 200.0f;
                edge_depth = edge_depth > depth_threshold ? 1 : 0;
                
                float normal_threshold = 1 / 4.0f;
                edge_normal = edge_normal > normal_threshold ? 1 : 0;
                
                float luminance_threshold = 1 / 0.5f;
                edge_luminance = edge_luminance > luminance_threshold ? 1 : 0;
                
                // Combine the edges from depth/normals/luminance using the max operator.
                float edge = max(edge_depth, max(edge_normal, edge_luminance));
                
                // Color the edge with a custom color.
                return edge * _OutlineColor;
                */
            }
            ENDHLSL
        }
    }
}