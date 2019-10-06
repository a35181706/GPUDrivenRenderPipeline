using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUDRP
{
    using MeshClusterRendering;

    [RequireComponent(typeof(Camera))]
    public class GPUDRPCamera : MonoBehaviour
    {

        public int AcutalWidth
        {
            get
            {
                return (int)(hostCamera.pixelWidth * GPUDrivenRenderingPipelineAssets.Instance.DynamicResScale);
            }
        }

        public int AcutalHeight
        {
            get
            {
                return (int)(hostCamera.pixelHeight * GPUDrivenRenderingPipelineAssets.Instance.DynamicResScale);
            }
        }

        public int NoScaleWidth
        {
            get
            {
                return hostCamera.pixelWidth;
            }
        }
        public int NoScaleHeight
        {
            get
            {
                return hostCamera.pixelHeight;
            }
        }

        /// <summary>
        /// 缓冲
        /// </summary>
        private GPUDRPBuffers RTBuffers = new GPUDRPBuffers();

        private Camera hostCamera;
        private ScriptableCullingParameters cullParams;

        public bool BeginRender()
        {
            if(!hostCamera)
            {
                hostCamera = GetComponent<Camera>();
            }

            //使用unity的视椎体裁剪
            if (!hostCamera.TryGetCullingParameters(out cullParams))
            {
                return false;
            }

            //这个会覆盖以及清空之前的commandbuffer设置
            PipelineContext.renderContext.SetupCameraProperties(hostCamera);

            return true;
        }


        public void Render()
        {
            SetUpBuffers();

            MCRRenderer.Execute();

            RenderNormal();

            PostProcessing();
        }


        private void SetUpBuffers()
        {
            RTBuffers.ResizeBufferIfNeed(this);

            PipelineContext.mainCmdBuffer.SetRenderTarget(RTBuffers.frameBuffer, RTBuffers.depthBuffer);
            PipelineContext.mainCmdBuffer.ClearRenderTarget(true, true, hostCamera.backgroundColor);

            PipelineContext.ExecuteMainCommandBuffer();
        }

        private void RenderNormal()
        {

            //绘制原来不透明物体,由于这里用的不是commandbuffer，所以要注意一下，在这个之前的commandbuffer需要先excure一遍
            var drawSettings = new DrawingSettings();
            drawSettings.SetShaderPassName(0, ShaderPass.c_UniltPass_ID);
            drawSettings.sortingSettings = new SortingSettings(hostCamera) { criteria = SortingCriteria.CommonOpaque };
            drawSettings.perObjectData = PerObjectData.None;
            var filterSettings = new FilteringSettings
            {
                layerMask = hostCamera.cullingMask,
                renderingLayerMask = 1,
                renderQueueRange = RenderQueueRange.opaque
            };

            CullingResults cullingResults = PipelineContext.renderContext.Cull(ref cullParams);

            PipelineContext.renderContext.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);
            PipelineContext.renderContext.DrawSkybox(hostCamera);
        }

        private void PostProcessing()
        {
            PipelineContext.mainCmdBuffer.Blit(RTBuffers.frameBuffer, hostCamera.targetTexture);

            PipelineContext.ExecuteMainCommandBuffer();
        }

        /// <summary>
        /// 渲染结束，用于释放一些本帧申请的临时资源
        /// </summary>
        public void EndRender()
        {

        }

        public void Clear()
        {
            RTBuffers.ClearBuffers();
        }
    }

}
