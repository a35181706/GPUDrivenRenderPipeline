using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP
{
    public static class GlobalMaterial
    {

        public static void Init()
        {
            GPUDrivenRenderingPipelineAssets.Instance.mcrAssets.InitGlobalMaterial();
            GPUDrivenRenderingPipelineAssets.Instance.gpuCullAssets.InitGlobalMaterial();
        }

        public static void Destroy()
        {
            GPUDrivenRenderingPipelineAssets.Instance.mcrAssets.DestroyGlobalMaterial();
            GPUDrivenRenderingPipelineAssets.Instance.gpuCullAssets.DestroyGlobalMaterial();
        }
    }

}
