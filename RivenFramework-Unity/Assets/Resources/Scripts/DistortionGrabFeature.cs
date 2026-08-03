using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class DistortionGrabFeature : ScriptableRendererFeature
{
    class GrabPass : ScriptableRenderPass
    {
        private static readonly int TexID = Shader.PropertyToID("_DistortionGrabTexture");
        private RTHandle _grabHandle;
        private string _profilerTag = "Distortion Grab Pass";

        public void Setup(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _grabHandle, desc, name: "_DistortionGrabTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraColor = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            if (_grabHandle == null || _grabHandle.rt == null) return;
            if (cameraColor == null || cameraColor.rt == null) return;
            
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            Blitter.BlitCameraTexture(cmd, cameraColor, _grabHandle);
            cmd.SetGlobalTexture(TexID, _grabHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            _grabHandle?.Release();
        }
    }

    public RenderPassEvent grabEvent = RenderPassEvent.AfterRenderingTransparents;

    private GrabPass _pass;

    public override void Create()
    {
        _pass = new GrabPass();
        _pass.Setup(grabEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
    }
}