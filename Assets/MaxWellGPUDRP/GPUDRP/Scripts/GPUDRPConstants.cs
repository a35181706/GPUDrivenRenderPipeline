using UnityEngine;
using UnityEngine.Rendering;
namespace MaxWellGPUDrivenRenderPipeline
{
    public static class GPUDRPPassNames
    {
        private const string c_UNILT_PASS_STR = "MaxWellGPUDRPUnilt";

        public readonly static ShaderTagId c_UNLIT_NAME= new ShaderTagId(c_UNILT_PASS_STR);
    }
}