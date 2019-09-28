using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxWellGPUDrivenRenderPipeline
{
    /// <summary>
    /// 全局材质，用于保存全局公用的材质
    /// </summary>
    public static class GlobalMaterial 
    {
        /// <summary>
        /// HiZDepth 生成LOD
        /// </summary>
        public static Material HiZDepthLODMat { get; private set; }
        /// <summary>
        /// HiZDepth，深度归一化到0~1
        /// </summary>
        public static Material HiZLinearLODMat { get; private set; }
        /// <summary>
        /// Cluster渲染材质
        /// </summary>
        public static Material ClusterRenderMat { get; private set; }

        private static bool bInit = false;
        public static void Init()
        {
            if(bInit)
            {
                return;
            }

            bInit = true;
            HiZDepthLODMat = new Material(MaxWellGPUDrivenRenderPipelineAssets.Instance.HiZLODShader);
            HiZLinearLODMat = new Material(MaxWellGPUDrivenRenderPipelineAssets.Instance.LinearDepthShader);
            ClusterRenderMat = new Material(MaxWellGPUDrivenRenderPipelineAssets.Instance.ClusterRenderShader);
        }


        public static void Clear()
        {
            CoreUtil.Destroy(HiZDepthLODMat);
            CoreUtil.Destroy(HiZLinearLODMat);
            CoreUtil.Destroy(ClusterRenderMat);
        }
    }

}
