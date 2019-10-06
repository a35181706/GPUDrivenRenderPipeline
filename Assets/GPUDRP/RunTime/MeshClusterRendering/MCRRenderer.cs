using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    public static class MCRRenderer 
    {
        private static List<MCRScene> renderList = new List<MCRScene>();
        private static List<uint> indirectArgs = new List<uint> { 0,0, 0, 0, 0 };
        public static void AddToRenderList(MCRScene scene)
        {
            if(renderList.Contains(scene))
            {
                return;
            }

            renderList.Add(scene);
        }

        public static void RemoveFromRenderList(MCRScene scene)
        {
            renderList.Remove(scene);
        }


        public static void Execute()
        {
            foreach(MCRScene scene in renderList)
            {
                if(scene.isLoadFinish)
                {
                    RenderScene(scene);
                }
            }
        }

        private static void RenderScene(MCRScene scene)
        {
            
            ComputeBuffer clusterBuffer = ComputeBufferPool.GetTempBuffer(scene.ClusterCount, scene.clusterStride);
            ComputeBuffer vertexBuffer = ComputeBufferPool.GetTempBuffer(scene.VertexCount, scene.vertexStride);
            ComputeBuffer indirectBuffer = ComputeBufferPool.GetIndirectBuffer();

            indirectArgs[0] = MCRConstant.CLUSTER_VERTEX_COUNT;
            indirectArgs[1] = (uint)scene.ClusterCount;

            clusterBuffer.SetData<ClusterInfo>(scene.clusterList);
            vertexBuffer.SetData<VertexInfo>(scene.vertexList);
            indirectBuffer.SetData<uint>(indirectArgs);

            PipelineContext.mainCmdBuffer.SetGlobalBuffer(MCRConstant._MCRVertexBuffer, vertexBuffer);
            PipelineContext.mainCmdBuffer.SetGlobalBuffer(MCRConstant._MCRClusterBuffer, clusterBuffer);

            PipelineContext.mainCmdBuffer.DrawProceduralIndirect(Matrix4x4.identity, scene.pipelineAsset.UnlitMat, 0, MeshTopology.Triangles, indirectBuffer);

            ComputeBufferPool.ReleaseEndOfRender(ref clusterBuffer);
            ComputeBufferPool.ReleaseEndOfRender(ref vertexBuffer);
            ComputeBufferPool.ReleaseEndOfRender(ref indirectBuffer);
        }
    }

}
