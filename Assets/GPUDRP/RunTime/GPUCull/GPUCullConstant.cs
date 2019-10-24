using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP.GPUCull
{
    public class GPUCullConstant 
    {
        public static readonly int _FrustumPlanes = Shader.PropertyToID("_FrustumPlanes");
        public static readonly int _GPUCullInfo = Shader.PropertyToID("_GPUCullInfo");

        
        public static readonly string FrustmCullKernelName = "FrustmCull";
        public static readonly string OcclusCullKernelName = "OcclusCull";
        public static readonly string ClearCullKernelName = "Clear";

        public const int FrustmCullNumThread = MeshClusterRendering.MCRConstant.CLUSTER_TRANGLES_COUNT;

    }

}
