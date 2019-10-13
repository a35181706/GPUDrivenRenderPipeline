using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    /// <summary>
    /// MCR资源
    /// </summary>
    [System.Serializable]
    public class MCRPipelineAssets 
    {
        public Shader UnlitShader;

        public Material UnlitMat { get; private set; }

        public void InitGlobalMaterial()
        {
            UnlitMat = new Material(UnlitShader);
        }

        public void DestroyGlobalMaterial()
        {
            CoreUtil.Destroy(UnlitMat);
        }


    }

}
