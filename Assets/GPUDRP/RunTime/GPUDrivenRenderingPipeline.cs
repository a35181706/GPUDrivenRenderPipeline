using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUDRP
{
    using MeshClusterRendering;

    public class GPUDrivenRenderingPipeline : UnityEngine.Rendering.RenderPipeline
    {
        private Dictionary<Camera, GPUDRPCamera> allGPUDRPCamera = new Dictionary<Camera, GPUDRPCamera>();

        private bool bFirstCall = false;

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            if(!bFirstCall)
            {
                if(Application.isPlaying)
                {
                    MCRResourcesSystem.Init();
                    GlobalMaterial.Init();
                }

                bFirstCall = true;
            }

            foreach(Camera cam in cameras)
            {
                GPUDRPCamera gpudrpCamera = null;

                if (!allGPUDRPCamera.TryGetValue(cam, out gpudrpCamera))
                {
                    gpudrpCamera = cam.GetComponent<GPUDRPCamera>();

                    if (!gpudrpCamera)
                    {
                        gpudrpCamera = cam.gameObject.AddComponent<GPUDRPCamera>();
                        allGPUDRPCamera.Add(cam, gpudrpCamera);
                    }
                }

                //设置pipelinecontext
                PipelineContext.mainCmdBuffer = CommandBufferPool.Get("MainCommandBuffer");
                PipelineContext.renderContext = context;
                PipelineContext.gpuCamera = gpudrpCamera;

                if(PipelineContext.gpuCamera.BeginRender())
                {
                    PipelineContext.gpuCamera.Render();
                }

                context.Submit();

                PipelineContext.gpuCamera.EndRender();

                ComputeBufferPool.EndOfRender();
                CommandBufferPool.Release(PipelineContext.mainCmdBuffer);
            }

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var cam in allGPUDRPCamera.Values)
            {
                cam.Clear();
            }

            allGPUDRPCamera.Clear();

            MCRResourcesSystem.Destroy();
            ComputeBufferPool.Destroy();
            CommandBufferPool.Destroy();
            GlobalMaterial.Destroy();
        }
    }

}
