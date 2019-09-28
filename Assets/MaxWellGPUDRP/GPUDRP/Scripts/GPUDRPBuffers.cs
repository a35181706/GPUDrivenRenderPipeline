using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace MaxWellGPUDrivenRenderPipeline
{
    /// <summary>
    /// GPUDRP的一些buffer，所有用到的buffer都保存在这里
    /// </summary>
    public class GPUDRPBuffers 
    {
        public RenderTexture frameBuffer { get; private set; }
        public RenderTexture depthBuffer { get; private set; }
        /// <summary>
        /// HiZ的历史深度贴图
        /// </summary>
        public RenderTexture historyDepth { get; private set; }

        /// <summary>
        /// 用于计算HiZ的mimip，临时贴图，因为RT不支持既做输入也做输出，需要一个中间RT
        /// </summary>
        public RenderTexture historyDepthIntermediateRT { get; private set; }
        public int2 HiZDepthSize;

        private int lastWidth = -1;
        private int lastHeight = -1;

        public void ResizeBufferIfNeed(GPUDRPCamera camera)
        {
            if (frameBuffer 
                && lastHeight == camera.AcutalHeight && lastWidth == camera.AcutalWidth)
            {
                return;
            }

           
            lastHeight = camera.AcutalHeight;
            lastWidth = camera.AcutalWidth;

            //framebuffer
            CoreUtil.Destroy(frameBuffer);
            frameBuffer = new RenderTexture(lastWidth, lastHeight, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.sRGB);
            frameBuffer.name = "GPUDRPFarmeBuffer";
            frameBuffer.Create();

            //depthBuffer
            CoreUtil.Destroy(depthBuffer);
            depthBuffer = new RenderTexture(lastWidth, lastHeight, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            depthBuffer.name = "GPUDRPDepthBuffer";
            depthBuffer.useMipMap = false;
            depthBuffer.autoGenerateMips = false;
            depthBuffer.enableRandomWrite = false;
            depthBuffer.wrapMode = TextureWrapMode.Clamp;
            depthBuffer.filterMode = FilterMode.Point;
            depthBuffer.Create();

            //HiZ DepthBuffer
            HiZDepthSize.x = 512;
            HiZDepthSize.y = (int)(((float)lastHeight / lastWidth) * HiZDepthSize.x );

            CoreUtil.Destroy(historyDepth);
            historyDepth = new RenderTexture(HiZDepthSize.x, HiZDepthSize.y, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            historyDepth.name = "HiZHistoryDepth";
            historyDepth.useMipMap = true;
            historyDepth.autoGenerateMips = false;
            historyDepth.enableRandomWrite = false;
            historyDepth.wrapMode = TextureWrapMode.Clamp;
            historyDepth.filterMode = FilterMode.Point;
            historyDepth.Create();

            //historyDepthIntermediateRT
            CoreUtil.Destroy(historyDepthIntermediateRT);
            historyDepthIntermediateRT = new RenderTexture(HiZDepthSize.x, HiZDepthSize.y,0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            historyDepthIntermediateRT.name = "historyDepthIntermediateRT";
            historyDepthIntermediateRT.useMipMap = true;
            historyDepthIntermediateRT.autoGenerateMips = false;
            historyDepthIntermediateRT.enableRandomWrite = false;
            historyDepthIntermediateRT.wrapMode = TextureWrapMode.Clamp;
            historyDepthIntermediateRT.filterMode = FilterMode.Point;
            historyDepthIntermediateRT.Create();

        }

        public void GenerateHistoryDepthMips(CommandBuffer buffer)
        {
            int mipCount = 1 + (int)Mathf.Log(Mathf.Max(historyDepth.width, historyDepth.height), 2.0f);
            buffer.SetGlobalTexture(ShaderID._MainTex, historyDepth);
            for (int i = 1; i < mipCount; ++i)
            {
                buffer.SetGlobalInt(ShaderID._PreviousLevel, i - 1);
                buffer.SetRenderTarget(historyDepthIntermediateRT, i);
                buffer.DrawMesh(GraphicsUtility.mesh, Matrix4x4.identity, GlobalMaterial.HiZDepthLODMat, 0, 0);
                buffer.CopyTexture(historyDepthIntermediateRT, 0, i, historyDepth, 0, i);
            }
        }

        public void ClearBuffers()
        {
            CoreUtil.Destroy(frameBuffer);
            CoreUtil.Destroy(historyDepth);
            CoreUtil.Destroy(historyDepthIntermediateRT);
        }
    }

}
