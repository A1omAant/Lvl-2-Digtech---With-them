using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTargetIdentifier source;
        private Material _material;

        private RTHandle tempRenderTarget;
        private RTHandle tempRenderTarget2;

        public CustomRenderPass(Material mat)
        {
            _material = mat;

            // Allocate RTHandles
            tempRenderTarget = RTHandles.Alloc(
                width: -1,
                height: -1,
                slices: 1,
                dimension: TextureDimension.Tex2D,
                colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                enableRandomWrite: false,
                useMipMap: false,
                autoGenerateMips: false,
                name: "_TemporaryColourTexture"
            );

            tempRenderTarget2 = RTHandles.Alloc(
                width: -1,
                height: -1,
                slices: 1,
                dimension: TextureDimension.Tex2D,
                colorFormat: GraphicsFormat.R8G8B8A8_SRGB,
                enableRandomWrite: false,
                useMipMap: false,
                autoGenerateMips: false,
                name: "_TemporaryDepthTexture"
            );

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Reflection)
            {
                CommandBuffer cmd = CommandBufferPool.Get();

                RTHandle sourceHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

                Blit(cmd, sourceHandle, tempRenderTarget, _material);
                Blit(cmd, tempRenderTarget, sourceHandle);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // No need to release tempRenderTarget here because RTHandles are persistent
        }

        public void Release()
        {
            // Call this when destroying the feature
            RTHandles.Release(tempRenderTarget);
            RTHandles.Release(tempRenderTarget2);
        }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();
    private CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (settings.material == null)
            settings.material = (Material)Resources.Load("Water_Volume");

        m_ScriptablePass = new CustomRenderPass(settings.material);
        m_ScriptablePass.renderPassEvent = settings.renderPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.source = renderer.cameraColorTargetHandle;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            m_ScriptablePass?.Release();
        }
        base.Dispose(disposing);
    }
}