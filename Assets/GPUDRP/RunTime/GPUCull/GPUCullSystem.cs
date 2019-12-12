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
            PipelineContext.mainCmdBuffer.BeginSample("GPU-Cull");
            ResetCullResult(context);
            FrustumCull(context);
            OcclusCull(context);

            PipelineContext.mainCmdBuffer.EndSample ("GPU-Cull");
            PipelineContext.ExecuteMainCommandBuffer();
        }

        private static void ResetCullResult(MCRSceneContext context)
        {
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader, ClearKernel, MCRConstant._MCRCullInstanceCountBuffer, context.cullInstanceCountBuffer);
            //清理buffer
            PipelineContext.mainCmdBuffer.DispatchCompute(cullShader, ClearKernel, 1, 1, 1);
        }

        private static void FrustumCull(MCRSceneContext context)
        {
            //设置buffer
            PipelineContext.mainCmdBuffer.SetComputeVectorArrayParam(cullShader, 
                                                                    GPUCull.GPUCullConstant._FrustumPlanes, 
                                                                    PipelineContext.gpuCamera.frustumPlane);
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader, FrustumKernerl, MCRConstant._MCRClusterBuffer, context.clusterBuffer);
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader, FrustumKernerl, MCRConstant._MCRCullResultBuffer, context.cullResultBuffer);
            PipelineContext.mainCmdBuffer.SetComputeBufferParam(cullShader, FrustumKernerl, MCRConstant._MCRCullInstanceCountBuffer, context.cullInstanceCountBuffer);

            int val = context.ClusterCount / GPUCullConstant.FrustmCullNumThreads;
            PipelineContext.mainCmdBuffer.DispatchCompute(cullShader, FrustumKernerl, val , 1, 1);
            //PipelineContext.mainCmdBuffer.DispatchCompute(cullShader, FrustumKernerl, val, 1, 1);
        }

        private static void OcclusCull(MCRSceneContext context)
        {

        }
    }

}
