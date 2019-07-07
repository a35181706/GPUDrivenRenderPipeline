using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDrivenRenderPipeline
{
    /// <summary>
    /// ShaderID
    /// </summary>
    public static class ShaderID 
    {
        //MCR
        
        public static readonly int clusterBuffer = Shader.PropertyToID("clusterBuffer");
        public static readonly int instanceCountBuffer = Shader.PropertyToID("instanceCountBuffer");
        public static readonly int resultBuffer = Shader.PropertyToID("resultBuffer");
        public static readonly int verticesBuffer = Shader.PropertyToID("verticesBuffer");
        public static readonly int dispatchBuffer = Shader.PropertyToID("dispatchBuffer");
        public static readonly int reCheckResult = Shader.PropertyToID("reCheckResult");
        public static readonly int reCheckCount = Shader.PropertyToID("reCheckCount");
        public static readonly int _IndexBuffer = Shader.PropertyToID("_IndexBuffer");

        //GPUClulling
        public static readonly int planes = Shader.PropertyToID("planes");
        public static readonly int _CameraUpVector = Shader.PropertyToID("_CameraUpVector");
        public static readonly int _HizDepthTex = Shader.PropertyToID("_HizDepthTex");
        public static readonly int _DepthBufferTexture = Shader.PropertyToID("_DepthBufferTexture");
        public static readonly int _Count = Shader.PropertyToID("_Count");
        public static readonly int _PreviousLevel = Shader.PropertyToID("_PreviousLevel");

        //其他
        public static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    }

}
