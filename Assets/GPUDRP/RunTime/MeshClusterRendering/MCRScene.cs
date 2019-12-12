using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
namespace GPUDRP.MeshClusterRendering
{
    public class MCRScene : MonoBehaviour
    {
        public bool CanRender = true;
        public MCRSceneContext context;

        public MCRPipelineAssets pipelineAsset
        {
            get
            {
                return GPUDrivenRenderingPipelineAssets.Instance.mcrAssets;
            }
        }

        private bool bFirstUpdate = false;
        public void Awake()
        {
            if(context.ClusterCount > 0 && context.VertexCount > 0)
            {
                context.Init(this);
                if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    MCRResourcesSystem.LoadMCRBakeAsset(context);
                }
                MCRRenderer.AddToRenderList(this);
               
            }

        }


        public void OnDestroy()
        {
            if (context.ClusterCount > 0 && context.VertexCount > 0)
            {
                MCRRenderer.RemoveFromRenderList(this);
                context.Destroy();
                context = null;
            }
        }


        public void Render()
        {
            if(!CanRender)
            {
                return;
            }
            GPUCull.GPUCullSystem.Cull(context);
            

            PipelineContext.mainCmdBuffer.SetGlobalBuffer(MCRConstant._MCRVertexBuffer, context.vertexBuffer);


            context.cullInstanceCountBuffer.GetData(context.instanceCount);

            if (context.instanceCount[0] <= 0)
            {
                return;
            }

            context.indirectArgs[0] = MCRConstant.CLUSTER_VERTEX_COUNT;
            context.indirectArgs[1] = context.instanceCount[0];

            context.inDirectBuffer.SetData<uint>(context.indirectArgs);

            PipelineContext.mainCmdBuffer.DrawProceduralIndirect(Matrix4x4.identity, pipelineAsset.UnlitMat, 0, MeshTopology.Triangles, context.inDirectBuffer);

        }

        public void BeforeRender()
        {
            if (!bFirstUpdate)
            {
                context.vertexBuffer.SetData<VertexInfo>(context.vertexList);
                context.clusterBuffer.SetData<ClusterInfo>(context.clusterList);

                bFirstUpdate = true;
            }
        }

        public void EndRender()
        {

        }
    }

}
