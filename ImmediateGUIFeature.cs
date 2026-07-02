using ImmediateShapes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ImmediateGUIFeature : ScriptableRendererFeature
{
    class ImmediateGUIPass : ScriptableRenderPass
    {
        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Immediate GUI");

            ScriptableGUIRenderer.RenderToScreen(cmd);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    ImmediateGUIPass pass;

    public override void Create()
    {
        pass = new ImmediateGUIPass();

        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}