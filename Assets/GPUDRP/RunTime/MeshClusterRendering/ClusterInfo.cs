using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    [System.Serializable]
    public unsafe struct ClusterInfo
    {
        public float4 center;
        public float3 extent;

        /// <summary>
        /// 这里存cluster的索引，而不存顶点的起始位置，是因为起始位置可能会出现更大的数字，比如超过65536，就需要更大的精度了
        /// 而存放cluster就可以精度少一点，比如16bit的最大值是65536，结合一个cluster64个三角形，就16bit的精度就可以存放400W左右的三角形
        /// 这样做的坏处就是，需要在cs中进行多一步乘法操作
        /// </summary>
        public int clusterindex;
    }

}
