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
                if(scene.isActiveAndEnabled && scene.context.bLoadFinish)
                {
                    RenderScene(scene);
                }
            }
        }

        private static void RenderScene(MCRScene scene)
        {
            scene.context.UpdateBuffers();

            PipelineContext.mainCmdBuffer.SetGlobalBuffer(MCRConstant._MCRVertexBuffer, scene.context.vertexBuffer);
            PipelineContext.mainCmdBuffer.SetGlobalBuffer(MCRConstant._MCRClusterBuffer, scene.context.clusterBuffer);

            PipelineContext.mainCmdBuffer.DrawProceduralIndirect(Matrix4x4.identity, scene.pipelineAsset.UnlitMat, 0, MeshTopology.Triangles, scene.context.inDirectBuffer);

        }
    }

}
