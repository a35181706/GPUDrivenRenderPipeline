using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaxWellGPUDrivenRenderPipeline
{
    public class MaxWellGPUDrivenRenderPipeline : UnityEngine.Rendering.RenderPipeline
    {
        private Dictionary<Camera,GPUDRPCamera> allGPUDRPCamera = new Dictionary<Camera,GPUDRPCamera>();

        public MaxWellGPUDrivenRenderPipeline()
        {
            allGPUDRPCamera.Clear();
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            GlobalMaterial.Init();

            foreach (var cam in cameras)
            {
                GPUDRPCamera gpudrpCamera = null;

                if(!allGPUDRPCamera.TryGetValue(cam,out gpudrpCamera))
                {
                    gpudrpCamera = cam.GetComponent<GPUDRPCamera>();

                    if (!gpudrpCamera)
                    {
                        gpudrpCamera = cam.gameObject.AddComponent<GPUDRPCamera>();
                        allGPUDRPCamera.Add(cam, gpudrpCamera);
                    }
                }

                var commandbuffer = CommandBufferPool.Get("GPUDRPMainCommandBuffer:" + cam.name);

                //设置渲染context
                GPUDRPRenderContext.Current.commandbuffer = commandbuffer;
                GPUDRPRenderContext.Current.gpudrpCamera = gpudrpCamera;
                GPUDRPRenderContext.Current.srpContex = context;

                if(gpudrpCamera.BeginRender(GPUDRPRenderContext.Current))
                {
                    gpudrpCamera.Render(GPUDRPRenderContext.Current);
                }

                context.Submit();

                gpudrpCamera.EndRender(GPUDRPRenderContext.Current);
                CommandBufferPool.Release(commandbuffer);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            GlobalMaterial.Clear();
            foreach (var cam in allGPUDRPCamera.Values)
            {
                cam.Clear();
            }

            allGPUDRPCamera.Clear();
        }
    }

}
