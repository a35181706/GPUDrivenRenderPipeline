using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
namespace GPUDRP.MeshClusterRendering
{
    public class MCRScene : MonoBehaviour
    {
        public MCRSceneContext context;

        public MCRPipelineAssets pipelineAsset
        {
            get
            {
                return GPUDrivenRenderingPipelineAssets.Instance.mcrAssets;
            }
        }

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
    }

}
