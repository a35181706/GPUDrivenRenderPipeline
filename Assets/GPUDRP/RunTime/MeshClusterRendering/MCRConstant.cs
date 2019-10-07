using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    public class MCRConstant 
    {

        /// <summary>
        /// 一个cluster有多少个三角形
        /// </summary>
        public const int CLUSTER_TRANGLES_COUNT = 64;

        /// <summary>
        /// 一个cluster有多少个顶点数据
        /// </summary>
        public const int CLUSTER_VERTEX_COUNT = CLUSTER_TRANGLES_COUNT * 3;

        public static readonly int _MCRVertexBuffer = Shader.PropertyToID("_MCRVertexBuffer");

        public static readonly int _MCRClusterBuffer = Shader.PropertyToID("_MCRClusterBuffer");

        /// <summary>
        /// MCR 资源你的保存路径
        /// </summary>
        public static readonly string BakeAssetSavePath = Application.streamingAssetsPath + "/MCRBakeAsset";

    }

}
