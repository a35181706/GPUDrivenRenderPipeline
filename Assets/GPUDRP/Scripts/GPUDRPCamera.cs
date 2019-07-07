using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUDrivenRenderPipeline
{
    [RequireComponent(typeof(Camera))]
    public class GPUDRPCamera : MonoBehaviour
    {
        /// <summary>
        /// 动态分辨率0.5~2.0f
        /// </summary>
        public const float DynamicResoulation = 1.0f;

        public int AcutalWidth
        {
            get
            {
                return (int)(hostCamera.pixelWidth * DynamicResoulation);
            }
        }

        public int AcutalHeight
        {
            get
            {
                return (int)(hostCamera.pixelHeight * DynamicResoulation);
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

        [HideInInspector]
        public Vector3 lastFrameCameraUp;

        [HideInInspector]
        public Camera hostCamera;
        /// <summary>
        /// 缓冲
        /// </summary>
        [HideInInspector]
        public GPUDRPBuffers RTBuffers;

        [System.NonSerialized]
        public Vector4[] frustumPlanes = new Vector4[6];

        private void Init()
        {
            hostCamera = GetComponent<Camera>();

            lastFrameCameraUp = Vector3.up;

            if(null == RTBuffers)
            {
                RTBuffers = new GPUDRPBuffers();
            }
        }

        /// <summary>
        /// 每一帧渲染开始之前都会调用
        /// </summary>
        public bool BeginRender(GPUDRPRenderContext context)
        {
            Init();            

            return true;
        }

        /// <summary>
        /// 渲染
        /// </summary>
        public void Render(GPUDRPRenderContext context)
        {
            ScriptableCullingParameters cullParams;

            //使用unity的视椎体裁剪
            if (!hostCamera.TryGetCullingParameters(out cullParams))
            {
                return;
            }

            for (int i = 0; i < frustumPlanes.Length; ++i)
            {
                Plane p = cullParams.GetCullingPlane(i);
                //GPUDRP的参见与SRP的裁剪是反向的
                frustumPlanes[i] = new Vector4(-p.normal.x, -p.normal.y, -p.normal.z, -p.distance);
            }

            RTBuffers.ResizeBufferIfNeed(this);

            //设置摄像机属性，如果不设置摄像机属性的话，就不会渲染啦~
            context.srpContex.SetupCameraProperties(hostCamera);

            //设置渲染的RT
            context.commandbuffer.SetRenderTarget(RTBuffers.frameBuffer,RTBuffers.depthBuffer);
            context.commandbuffer.ClearRenderTarget(true, true, hostCamera.backgroundColor);

            
            MCRExecuterManager.DrawAllExecuter(this);
             

            //由于MCR绘制会改变RT,这里需要把RT重新设置回来
            context.commandbuffer.SetRenderTarget(RTBuffers.frameBuffer, RTBuffers.depthBuffer);
            context.srpContex.ExecuteCommandBuffer(context.commandbuffer);

            DrawUnityBuiltInRenderer(context, ref cullParams);

            var blitCmd = CommandBufferPool.Get(hostCamera.name + ":FinalDynamicResolutionBlit");

            blitCmd.Blit(RTBuffers.frameBuffer, hostCamera.targetTexture);
            context.srpContex.ExecuteCommandBuffer(blitCmd);

            CommandBufferPool.Release(blitCmd);
        }

        /// <summary>
        /// 绘制Unity内置渲染器
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cullParams"></param>
        /// <returns></returns>
        private ScriptableCullingParameters DrawUnityBuiltInRenderer(GPUDRPRenderContext context, ref ScriptableCullingParameters cullParams)
        {
  
            var drawSettings = new DrawingSettings();

            drawSettings.SetShaderPassName(0, GPUDRPPassNames.c_UNLIT_NAME);
            drawSettings.sortingSettings = new SortingSettings(hostCamera) { criteria = SortingCriteria.CommonOpaque };
            drawSettings.perObjectData = PerObjectData.None;
            var filterSettings = new FilteringSettings
            {
                layerMask = hostCamera.cullingMask,
                renderingLayerMask = 1,
                renderQueueRange = RenderQueueRange.opaque
            };

            CullingResults cullingResults = context.srpContex.Cull(ref cullParams);

            //绘制原来的物体
            context.srpContex.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

            //绘制天空盒
            context.srpContex.DrawSkybox(hostCamera);
            return cullParams;
        }

        /// <summary>
        /// 结束渲染，可用于释放本帧资源
        /// </summary>
        public void EndRender(GPUDRPRenderContext context)
        {

        }

        public void Clear()
        {
            RTBuffers.ClearBuffers();
        }
    }
}

