using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUDRP
{
    public class GPUDrivenRenderingPipelineAssets : UnityEngine.Rendering.RenderPipelineAsset
    {

#if UNITY_EDITOR
        [MenuItem("Assets/GPUDRP/CreateAssets")]
        public static void CreateAssets()
        {
            GPUDrivenRenderingPipelineAssets asset = ScriptableObject.CreateInstance<GPUDrivenRenderingPipelineAssets>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/New GPUDRPAsset.asset"));
        }
#endif

        public GPUDRP.MeshClusterRendering.MCRPipelineAssets mcrAssets;
        public float DynamicResScale = 1;

        public static GPUDrivenRenderingPipelineAssets Instance
        {
            get
            {
                return m_Instance;
            }
        }
        private static GPUDrivenRenderingPipelineAssets m_Instance;

        protected override RenderPipeline CreatePipeline()
        {
            m_Instance = this;
            return new GPUDrivenRenderingPipeline();
        }
    }

}
