using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class EdgeDetection : ScriptableRendererFeature
{
     private class EdgeDetectionPass : ScriptableRenderPass
    {
        private Material material;

        private static readonly int OutlineThicknessProperty = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        
        // Camera depth
        private static readonly int DepthFalloffProperty = Shader.PropertyToID("_DepthFalloff");
        private static readonly int MinThicknessProperty = Shader.PropertyToID("_MinThickness");

        public EdgeDetectionPass()
        {
            profilingSampler = new ProfilingSampler(nameof(EdgeDetectionPass));
        }

        public void Setup(ref EdgeDetectionSettings settings, ref Material edgeDetectionMaterial)
        {
            material = edgeDetectionMaterial;
            renderPassEvent = settings.renderPassEvent;

            material.SetFloat(OutlineThicknessProperty, settings.outlineThickness);
            material.SetColor(OutlineColorProperty, settings.outlineColor);
            
            // Camera depth
            material.SetFloat(DepthFalloffProperty, settings.depthFalloff);
            material.SetFloat(MinThicknessProperty, settings.minThickness);
            
            // Hand-drawn
            material.SetFloat("_WobbleStrength", settings.wobbleStrength);
            material.SetFloat("_WobbleFrequency", settings.wobbleFrequency);
        }

        private class PassData
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            using var builder = renderGraph.AddRasterRenderPass<PassData>("Edge Detection", out _);

            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.UseAllGlobalTextures(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc((PassData _, RasterGraphContext context) => { Blitter.BlitTexture(context.cmd, Vector2.one, material, 0); });
        }
    }

    [Serializable]
    public class EdgeDetectionSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        [Range(0, 5)] public float outlineThickness = 1;
        public Color outlineColor = Color.black;
        
        // Camera depth
        [Range(0.01f, 1.0f)] public float depthFalloff = 0.1f;
        [Range(0.0f, 5.0f)] public float minThickness = 0.5f;
        
        // Hand-drawn
        [Range(0f, 0.05f)] public float wobbleStrength = 0.002f;
        [Range(1f, 100f)] public float wobbleFrequency = 10.0f;
    }

    [SerializeField] private EdgeDetectionSettings settings;
    private Material edgeDetectionMaterial;
    private EdgeDetectionPass edgeDetectionPass;

    /// <summary>
    /// Called
    /// - When the Scriptable Renderer Feature loads the first time.
    /// - When you enable or disable the Scriptable Renderer Feature.
    /// - When you change a property in the Inspector window of the Renderer Feature.
    /// </summary>
    public override void Create()
    {
        edgeDetectionPass ??= new EdgeDetectionPass();
    }

    /// <summary>
    /// Called
    /// - Every frame, once for each camera.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Don't render for some views.
        if (renderingData.cameraData.cameraType == CameraType.Preview
            || renderingData.cameraData.cameraType == CameraType.Reflection
            || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
            return;
        
        if (edgeDetectionMaterial == null)
        {
            edgeDetectionMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Outline/Edge Detection"));
            if (edgeDetectionMaterial == null)
            {
                Debug.LogWarning("Not all required materials could be created. Edge Detection will not render.");
                return;
            }
        }

        edgeDetectionPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
        edgeDetectionPass.requiresIntermediateTexture = true;
        edgeDetectionPass.Setup(ref settings, ref edgeDetectionMaterial);

        renderer.EnqueuePass(edgeDetectionPass);
    }

    /// <summary>
    /// Clean up resources allocated to the Scriptable Renderer Feature such as materials.
    /// </summary>
    override protected void Dispose(bool disposing)
    {
        edgeDetectionPass = null;
        CoreUtils.Destroy(edgeDetectionMaterial);
    }
}
