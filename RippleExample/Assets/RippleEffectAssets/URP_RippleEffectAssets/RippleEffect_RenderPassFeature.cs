using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RippleEffect_RenderPassFeature : ScriptableRendererFeature
{
    class RippleEffect_RenderPass : ScriptableRenderPass
    {
        static readonly int TempTargetId = Shader.PropertyToID("_Temp_RippleBlit"); // You can name this anything you want

        public RenderTargetIdentifier source;
        public RenderTargetHandle destination;
        public int blitShaderPassIndex;
        public FilterMode filterMode;

        private RenderTargetHandle m_TemporaryColorTexture;

        public RippleEffect_RenderPass(RenderPassEvent renderPassEvent, FilterMode filterMode, int blitShaderPassIndex)
        {
            this.renderPassEvent = renderPassEvent;
            this.filterMode = filterMode;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture"); // You can name this anything you want
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            if (Application.isPlaying && RipplePostProcessor.ins.CurrentAmount > RipplePostProcessor.LOWEST_AMOUNT_VALUE)
            {
                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

                // Can't read and write to same color target, use a TemporaryRT
                if (destination == RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                    Blit(cmd, source, m_TemporaryColorTexture.Identifier(), RipplePostProcessor.ins.RippleMaterial, blitShaderPassIndex);
                    Blit(cmd, m_TemporaryColorTexture.Identifier(), source);
                }

            }
            // execution
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    RippleEffect_RenderPass m_ScriptablePass;

    /// <summary>Configures where the render pass should be injected.</summary>
    public FilterMode filterMode = FilterMode.Bilinear;
    public RenderPassEvent TheRenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    public int blitShaderPassIndex = -1;
    public override void Create()
    {
        m_ScriptablePass = new RippleEffect_RenderPass(TheRenderPassEvent, filterMode, blitShaderPassIndex);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.source = renderer.cameraColorTarget;
        m_ScriptablePass.destination = RenderTargetHandle.CameraTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}