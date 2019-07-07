using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Text;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.InteropServices;

namespace GPUDrivenRenderPipeline
{
    /// <summary>
    /// Mesh Cluster Rendering 执行者，每个场景只能有 一个
    /// 负责MCR的绘制,有SceneClusterGenerator生成
    /// </summary>
    public unsafe class MCRExecuter : MonoBehaviour
    {
        public int clusterInfoTableIndex = -1;

        public bool useGPUOcclusCulling = true;

        private MCRExecuterContext context = null;

        private SceneStreamingSysetm streamlingSystem = null;

        private static MCRExecuterContext InitContext(MCRClusterInfo info,bool useGPUOcclusCulling)
        {
            if ( info.clusterCount <= 0)
            {
                return null;
            }

            MCRExecuterContext context = new MCRExecuterContext();
            context.clusterBuffer = new ComputeBuffer(info.clusterCount, Marshal.SizeOf<MCRCluster>());
            context.resultBuffer = new ComputeBuffer(info.clusterCount, Marshal.SizeOf<uint>());
            context.instanceCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            NativeArray<uint> instanceCountBufferValue = new NativeArray<uint>(5, Allocator.Temp);
            instanceCountBufferValue[0] = MCRConstant.c_CLUSTER_VERTEX_COUNT;
            context.instanceCountBuffer.SetData(instanceCountBufferValue);
            context.moveCountBuffer = new ComputeBuffer(5, Marshal.SizeOf<int>(), ComputeBufferType.IndirectArguments);
            context.verticesBuffer = new ComputeBuffer(info.clusterCount *2 * MCRConstant.c_CLUSTER_CLIP_COUNT, Marshal.SizeOf<MCRVertex>());
            context.clusterCount = 0;
            if (useGPUOcclusCulling)
            {
                context.reCheckCount = new ComputeBuffer(5, Marshal.SizeOf<int>(), ComputeBufferType.IndirectArguments);
                context.dispatchBuffer = new ComputeBuffer(5, Marshal.SizeOf<int>(), ComputeBufferType.IndirectArguments);
                context.reCheckResult = new ComputeBuffer(context.resultBuffer.count, sizeof(uint));
                context.reCheckCount.SetData(instanceCountBufferValue);
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemClear(instanceCountBufferValue.GetUnsafePtr(), 5 * Marshal.SizeOf<int>());
                instanceCountBufferValue[1] = 1;
                instanceCountBufferValue[2] = 1;
                context.dispatchBuffer.SetData(instanceCountBufferValue);
            }

            instanceCountBufferValue.Dispose();

            return context;
        }

        private static void DisposeContext(MCRExecuterContext context, bool useGPUOcclusCulling)
        {
            if(null == context)
            {
                return;
            }   
            void DisposeBuffer(ComputeBuffer bf)
            {
                if (bf != null) bf.Dispose();
            }
            DisposeBuffer(context.verticesBuffer);
            DisposeBuffer(context.clusterBuffer);
            if (useGPUOcclusCulling)
            {
                DisposeBuffer(context.reCheckCount);
                DisposeBuffer(context.reCheckResult);
                DisposeBuffer(context.dispatchBuffer);
            }
            DisposeBuffer(context.instanceCountBuffer);
            DisposeBuffer(context.resultBuffer);
            DisposeBuffer(context.moveCountBuffer);
        }

        public void Awake()
        {
           
            MCRClusterInfo info = MCRExecuterManager.GetMCRClusterInfo(clusterInfoTableIndex);

            if (info.clusterCount <= 0)
            {
                return;
            }

            context = InitContext(info, useGPUOcclusCulling);
            streamlingSystem = new SceneStreamingSysetm(info, clusterInfoTableIndex, context);
            MCRExecuterManager.AddExecuter(this);

        }

        public void Start()
        {
            StartCoroutine(streamlingSystem.Generate());
        }

        public void OnDestroy()
        {
            MCRExecuterManager.RemoveExecuter(this);
            DisposeContext(context, useGPUOcclusCulling);
        }

        public void DrawMCR(GPUDRPCamera gpudrpCamera)
        {
            if (context.clusterCount <= 0)
            {
                return;
            }

            
            CommandBuffer buffer = GPUDRPRenderContext.Current.commandbuffer; 
            ComputeShader gpuFrustumShader = GPUDrivenRenderPipelineAssets.Instance.gpuFrustumCullingShader;
            buffer.SetGlobalBuffer(ShaderID.resultBuffer, context.resultBuffer);
            buffer.SetGlobalBuffer(ShaderID.verticesBuffer, context.verticesBuffer);
            buffer.SetComputeBufferParam(gpuFrustumShader, MCRConstant.ClearCluster_Kernel, ShaderID.instanceCountBuffer, context.instanceCountBuffer);
            buffer.DispatchCompute(gpuFrustumShader, MCRConstant.ClearCluster_Kernel, 1, 1, 1);
            UpdateGPUCullingParams(context, gpudrpCamera,gpuFrustumShader, buffer);
            //First Draw来
            buffer.DrawProceduralIndirect(Matrix4x4.identity, GlobalMaterial.ClusterRenderMat, 0, MeshTopology.Triangles, context.instanceCountBuffer, 0);

            buffer.BlitSRT(gpudrpCamera.RTBuffers.historyDepth, GlobalMaterial.HiZLinearLODMat, 0);

            gpudrpCamera.RTBuffers.GenerateHiZDepthMips(buffer);

            //double check
            ClearOcclusionData(context, buffer, gpuFrustumShader);
            gpudrpCamera.lastFrameCameraUp = gpudrpCamera.hostCamera.transform.up;

            OcclusionRecheck(context, gpudrpCamera, gpuFrustumShader, buffer);

            //double draw
            buffer.SetRenderTarget(gpudrpCamera.RTBuffers.frameBuffer);
            buffer.DrawProceduralIndirect(Matrix4x4.identity, GlobalMaterial.ClusterRenderMat, 0, MeshTopology.Triangles, context.reCheckCount, 0);
            buffer.BlitSRT(gpudrpCamera.RTBuffers.historyDepth, GlobalMaterial.HiZLinearLODMat, 0);
            gpudrpCamera.RTBuffers.GenerateHiZDepthMips(buffer);

        }


        public static void UpdateGPUCullingParams(MCRExecuterContext context, GPUDRPCamera gpudrpCamera,
                                                    ComputeShader coreShader, CommandBuffer buffer)
        {
            buffer.SetComputeVectorArrayParam(coreShader, ShaderID.planes, gpudrpCamera.frustumPlanes);
            buffer.SetComputeVectorParam(coreShader, ShaderID._CameraUpVector, gpudrpCamera.lastFrameCameraUp);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID.clusterBuffer, context.clusterBuffer);
            buffer.SetComputeTextureParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID._HizDepthTex, gpudrpCamera.RTBuffers.historyDepth);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID.dispatchBuffer, context.dispatchBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID.resultBuffer, context.resultBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID.instanceCountBuffer, context.instanceCountBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.FrustumFilter_Kernel, ShaderID.reCheckResult, context.reCheckResult);
            ComputeShaderUtility.Dispatch(coreShader, buffer, MCRConstant.FrustumFilter_Kernel, context.clusterCount);
        }

        public static void OcclusionRecheck(MCRExecuterContext context, GPUDRPCamera gpudrpCamera,
                                            ComputeShader coreShader, CommandBuffer buffer)
        {
            buffer.SetComputeVectorParam(coreShader, ShaderID._CameraUpVector, gpudrpCamera.lastFrameCameraUp);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID.dispatchBuffer, context.dispatchBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID.reCheckResult, context.reCheckResult);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID.clusterBuffer, context.clusterBuffer);
            buffer.SetComputeTextureParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID._HizDepthTex, gpudrpCamera.RTBuffers.historyDepth);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID.reCheckCount, context.reCheckCount);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.OcclusionRecheck_Kernel, ShaderID.resultBuffer, context.resultBuffer);
            buffer.DispatchCompute(coreShader, MCRConstant.OcclusionRecheck_Kernel, context.dispatchBuffer, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearOcclusionData(MCRExecuterContext context, CommandBuffer buffer, ComputeShader coreShader)
        {
            buffer.SetComputeBufferParam(coreShader, MCRConstant.ClearOcclusionData_Kernel, ShaderID.dispatchBuffer, context.dispatchBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.ClearOcclusionData_Kernel, ShaderID.instanceCountBuffer, context.instanceCountBuffer);
            buffer.SetComputeBufferParam(coreShader, MCRConstant.ClearOcclusionData_Kernel, ShaderID.reCheckCount, context.reCheckCount);
            buffer.DispatchCompute(coreShader, MCRConstant.ClearOcclusionData_Kernel, 1, 1, 1);
        }
    }

}
