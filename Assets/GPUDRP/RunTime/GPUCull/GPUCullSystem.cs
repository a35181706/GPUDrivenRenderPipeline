using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUDRP.MeshClusterRendering;
namespace GPUDRP.GPUCull
{
    public static class GPUCullSystem
    {
        private static bool bInit = false;

        private static int FrustumKernerl = -1;
        private static int OccKernel = -1;
        private static int ClearKernel = -1;

        private static ComputeShader cullShader = null;

        /// <summary>
        /// GPU裁剪信息，x-cluster数目，yzw-尚未启用
        /// </summary>
        private static Vector4 gpuCullInfo;
        public static void Init()
        {
            if(bInit)
            {
                return;
            }

            cullShader = GPUDrivenRenderingPipelineAssets.Instance.gpuCullAssets.CullingShader; 

            if(!cullShader)
            {
                return;
            }

            bInit = true;

            FrustumKernerl = cullShader.FindKernel(GPUCullConstant.FrustmCullKernelName);
            OccKernel = cullShader.FindKernel(GPUCullConstant.OcclusCullKernelName);
            ClearKernel = cullShader.FindKernel(GPUCullConstant.ClearCullKernelName);

        }

        public static void Destroy()
        {
            if(!bInit)
            {
                return;
            }

            bInit = false;
        }

        public static void Cull(MCRSceneContext context)
        {
            if(!bInit)
            {
                return;
            }
            //清理buffer
            PipelineContext.mainCmdBuffer.DispatchCompute(cullShader, ClearKernel, 1, 1, 1);
            //加0.001，避免精度丢失问题
            gpuCullInfo.x = context.ClusterCount + 0.001f;

            PipelineContext.mainCmdBuffer.SetComputeVectorParam(cullShader, GPUCull.GPUCullConstant._GPUCullInfo, gpuCullInfo);

            FrustumCull(context);
            OcclusCull(context);
        }

        private static void FrustumCull(MCRSceneContext context)
        {
            //设置buffer
            PipelineContext.mainCmdBuffer.SetComputeVectorArrayParam(cullShader,GPUCull.GPUCullConstant._FrustumPlanes, PipelineContext.gpuCamera.frustumPlane);
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader,FrustumKernerl,MCRConstant._MCRClusterBuffer, context.clusterBuffer);
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader, FrustumKernerl, MCRConstant._MCRCullResultBuffer, context.cullResultBuffer);

            //PipelineContext.mainCmdBuffer.DispatchCompute(cullShader, FrustumKernerl, 1, 1, 1);
        }

        private static void OcclusCull(MCRSceneContext context)
        {

        }
    }

}
