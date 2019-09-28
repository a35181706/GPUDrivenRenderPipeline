using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaxWellGPUDrivenRenderPipeline
{
    
    public class MaxWellGPUDrivenRenderPipelineAssets : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [MenuItem("Assets/MaxWellGPUDRP/CreateAssets")]
        public static void CreateAssets()
        {
            MaxWellGPUDrivenRenderPipelineAssets asset = ScriptableObject.CreateInstance<MaxWellGPUDrivenRenderPipelineAssets>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/New MaxWellGPUDRPAsset.asset"));
        }
#endif

        public Shader UniltShader = null;
        public ComputeShader streamingShader = null;
        public ComputeShader gpuFrustumCullingShader = null;
        public Shader HiZLODShader = null;
        public Shader LinearDepthShader = null;
        public Shader ClusterRenderShader = null;
        public static MaxWellGPUDrivenRenderPipelineAssets Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private static MaxWellGPUDrivenRenderPipelineAssets m_Instance;

        protected override RenderPipeline CreatePipeline()
        {
            m_Instance = this;
            return new MaxWellGPUDrivenRenderPipeline();
        }
    }

}
