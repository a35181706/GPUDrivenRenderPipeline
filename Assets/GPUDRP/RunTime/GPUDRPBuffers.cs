using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace GPUDRP
{
    /// <summary>
    /// GPUDRP的一些buffer，所有用到的buffer都保存在这里
    /// </summary>
    public class GPUDRPBuffers 
    {
        public RenderTexture frameBuffer { get; private set; }
        public RenderTexture depthBuffer { get; private set; }


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
            frameBuffer.name = camera.name + "_FarmeBuffer";
            frameBuffer.Create();

            //depthBuffer
            CoreUtil.Destroy(depthBuffer);
            depthBuffer = new RenderTexture(lastWidth, lastHeight, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            depthBuffer.name = camera.name + "_DepthBuffer";
            depthBuffer.useMipMap = false;
            depthBuffer.autoGenerateMips = false;
            depthBuffer.enableRandomWrite = false;
            depthBuffer.wrapMode = TextureWrapMode.Clamp;
            depthBuffer.filterMode = FilterMode.Point;
            depthBuffer.Create();

        }



        public void ClearBuffers()
        {
            CoreUtil.Destroy(frameBuffer);
            CoreUtil.Destroy(depthBuffer);
        }
    }

}
