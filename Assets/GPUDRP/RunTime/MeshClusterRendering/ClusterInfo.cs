using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace GPUDRP.MeshClusterRendering
{
    [System.Serializable]
    public unsafe struct ClusterInfo
    {
        public float4 center;
        public float4 size;

        public int vertexStartIndex;
    }

}
