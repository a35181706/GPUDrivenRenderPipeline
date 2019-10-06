using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    public class MCRSceneContext
    {
        public List<uint> indirectArgs = null;
        public ComputeBuffer vertexBuffer = null;
        public ComputeBuffer clusterBuffer = null;
        public ComputeBuffer inDirectBuffer = null;

        private bool bFirstUpdate = false; 

        public void Init(MCRScene scene)
        {
            indirectArgs = new List<uint> { 0, 0, 0, 0, 0 };
            vertexBuffer = ComputeBufferPool.GetTempBuffer(scene.VertexCount, scene.vertexStride);
            clusterBuffer = ComputeBufferPool.GetTempBuffer(scene.ClusterCount, scene.clusterStride);
            inDirectBuffer = ComputeBufferPool.GetIndirectBuffer();
        }

        public void UpdateBuffers(MCRScene scene)
        {
            if(!bFirstUpdate)
            {
                vertexBuffer.SetData<VertexInfo>(scene.vertexList);
                clusterBuffer.SetData<ClusterInfo>(scene.clusterList);

                indirectArgs[0] = MCRConstant.CLUSTER_VERTEX_COUNT;
                indirectArgs[1] = (uint)scene.ClusterCount;
                inDirectBuffer.SetData<uint>(indirectArgs);

                bFirstUpdate = true;
            }
        }


        public void Destroy()
        {
            ComputeBufferPool.ReleaseTempBuffer(ref vertexBuffer);
            ComputeBufferPool.ReleaseTempBuffer(ref clusterBuffer);
            ComputeBufferPool.ReleaseTempBuffer(ref inDirectBuffer);
            indirectArgs = null;
            bFirstUpdate = false;
        }

    }

}
