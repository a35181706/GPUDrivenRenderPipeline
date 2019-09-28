using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUDRP
{
    public class GPUDrivenRenderingPipeline : UnityEngine.Rendering.RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
            foreach(Camera cam in cameras)
            {

                ScriptableCullingParameters cullParams;

                //使用unity的视椎体裁剪
                if (!cam.TryGetCullingParameters(out cullParams))
                {
                    return;
                }


                context.SetupCameraProperties(cam);

                var drawSettings = new DrawingSettings();

                drawSettings.SetShaderPassName(0, ShaderPass.c_UniltPass_ID);
                drawSettings.sortingSettings = new SortingSettings(cam) { criteria = SortingCriteria.CommonOpaque };
                drawSettings.perObjectData = PerObjectData.None;
                var filterSettings = new FilteringSettings
                {
                    layerMask = cam.cullingMask,
                    renderingLayerMask = 1,
                    renderQueueRange = RenderQueueRange.opaque
                };

                CullingResults cullingResults = context.Cull(ref cullParams);

                //绘制原来的物体
                context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

                context.DrawSkybox(cam);
                context.Submit();
            }

        }
    }

}
