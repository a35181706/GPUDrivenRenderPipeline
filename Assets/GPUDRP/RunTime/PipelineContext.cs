using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUDRP
{
    public static class PipelineContext
    {
        public static CommandBuffer mainCmdBuffer;
        public static ScriptableRenderContext renderContext;
        public static GPUDRPCamera gpuCamera;

        public static void ExecuteMainCommandBuffer()
        {
            if (mainCmdBuffer == null || mainCmdBuffer.sizeInBytes <= 0)
            {
                return;
            }

            renderContext.ExecuteCommandBuffer(mainCmdBuffer);
            mainCmdBuffer.Clear();
        }

       
    }

}
