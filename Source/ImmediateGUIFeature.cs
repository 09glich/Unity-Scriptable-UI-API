using ImmediateShapes;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;


public class ImmediateGUIFeature : ScriptableRendererFeature
{
    private class PassData { 
    }

    class ImmediateGUIPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (
                var builder =
                renderGraph.AddRasterRenderPass<PassData>(
                    "Immediate GUI", out var passData)
                ) {

                var Resources = frameData.Get<UniversalResourceData>();


                builder.SetRenderAttachment(Resources.activeColorTexture,0);


                builder.SetRenderFunc(
                (PassData data, RasterGraphContext context) =>
                {
                    ScriptableGUIRenderer.RenderToScreen(context.cmd);
                });
            }
            

           


           
        }


    }

    ImmediateGUIPass pass;

    public override void Create()
    {
        Debug.Log("Creating render feature");
        pass = new ImmediateGUIPass();

        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (!Application.isPlaying)
            return;

        renderer.EnqueuePass(pass);
    }
}