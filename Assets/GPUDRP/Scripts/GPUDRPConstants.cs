using UnityEngine;
using UnityEngine.Rendering;
namespace GPUDrivenRenderPipeline
{
    public static class GPUDRPPassNames
    {
        private const string c_UNILT_PASS_STR = "GPUDRPUnilt";

        public readonly static ShaderTagId c_UNLIT_NAME= new ShaderTagId(c_UNILT_PASS_STR);
    }
}