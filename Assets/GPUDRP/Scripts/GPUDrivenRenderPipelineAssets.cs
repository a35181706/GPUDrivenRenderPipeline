using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GPUDrivenRenderPipeline
{


    public class GPUDrivenRenderPipelineAssets : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [MenuItem("Assets/GPUDRP/CreateAssets")]
        public static void CreateAssets()
        {
            GPUDrivenRenderPipelineAssets asset = ScriptableObject.CreateInstance<GPUDrivenRenderPipelineAssets>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath("Assets/New GPUDRPAsset.asset"));
        }
#endif

        public Shader UniltShader = null;
        public ComputeShader streamingShader = null;
        public ComputeShader gpuFrustumCullingShader = null;
        public Shader HiZLODShader = null;
        public Shader LinearDepthShader = null;
        public Shader ClusterRenderShader = null;
        public static GPUDrivenRenderPipelineAssets Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private static GPUDrivenRenderPipelineAssets m_Instance;

        protected override RenderPipeline CreatePipeline()
        {
            m_Instance = this;
            return new GPUDrivenRenderPipeline();
        }
    }

}
