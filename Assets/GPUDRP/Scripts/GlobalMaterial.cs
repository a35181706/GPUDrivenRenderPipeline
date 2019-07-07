﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDrivenRenderPipeline
{
    /// <summary>
    /// 全局材质，用于保存全局公用的材质
    /// </summary>
    public static class GlobalMaterial 
    {
        /// <summary>
        /// HiZDepth LOD生成材质
        /// </summary>
        public static Material HiZDepthLODMat { get; private set; }
        /// <summary>
        /// 线性LOD
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
            HiZDepthLODMat = new Material(GPUDrivenRenderPipelineAssets.Instance.HiZLODShader);
            HiZLinearLODMat = new Material(GPUDrivenRenderPipelineAssets.Instance.LinearDepthShader);
            ClusterRenderMat = new Material(GPUDrivenRenderPipelineAssets.Instance.ClusterRenderShader);
        }


        public static void Clear()
        {
            CoreUtil.Destroy(HiZDepthLODMat);
            CoreUtil.Destroy(HiZLinearLODMat);
            CoreUtil.Destroy(ClusterRenderMat);
        }
    }

}
