using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace MaxWellGPUDrivenRenderPipeline
{
    /// <summary>
    /// GPUDRP渲染上下文
    /// </summary>
    public class GPUDRPRenderContext 
    {
        /// <summary>
        /// 当前实例
        /// </summary>
        public static GPUDRPRenderContext Current
        {
            get
            {
                if(null == m_Instance)
                {
                    m_Instance = new GPUDRPRenderContext();
                }

                return m_Instance;
            }
        }

        private static GPUDRPRenderContext m_Instance;

        /// <summary>
        /// 当前的commandbuffer
        /// </summary>
        public CommandBuffer commandbuffer;

        /// <summary>
        /// 当前的rendercontext
        /// </summary>
        public ScriptableRenderContext srpContex;

        /// <summary>
        /// 当前渲染的摄像机
        /// </summary>
        public GPUDRPCamera gpudrpCamera;

    }

}
