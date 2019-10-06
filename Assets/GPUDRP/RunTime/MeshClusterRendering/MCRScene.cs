using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
namespace GPUDRP.MeshClusterRendering
{
    public class MCRScene : MonoBehaviour
    {
        public string ClusterInfoAssetsPath = string.Empty;

        public int ClusterCount = 0;
        public int VertexCount = 0;

        [System.NonSerialized]
        public NativeArray<ClusterInfo> clusterList;
        [System.NonSerialized]
        public int clusterStride = Marshal.SizeOf<ClusterInfo>();

        [System.NonSerialized]
        public NativeArray<VertexInfo> vertexList;
        [System.NonSerialized]
        public int vertexStride = Marshal.SizeOf<VertexInfo>();

        public MCRSceneContext context { get; private set; }

        public bool isLoadFinish
        {
            get; set;
        }

        public MCRPipelineAssets pipelineAsset
        {
            get
            {
                return GPUDrivenRenderingPipelineAssets.Instance.mcrAssets;
            }
        }

        public void Awake()
        {
            //不能在另外的线程分配内容，只能在主线程
            clusterList = new Unity.Collections.NativeArray<ClusterInfo>(ClusterCount, Unity.Collections.Allocator.Persistent);
            vertexList = new Unity.Collections.NativeArray<VertexInfo>(VertexCount, Unity.Collections.Allocator.Persistent);

            if(ClusterCount > 0 && VertexCount > 0)
            {
                MCRResourcesSystem.LoadMCRBakeAsset(this);
                MCRRenderer.AddToRenderList(this);
                context = new MCRSceneContext();
                context.Init(this);
            }

        }



        public void OnDestroy()
        {
            if (ClusterCount > 0 && VertexCount > 0)
            {
                MCRRenderer.RemoveFromRenderList(this);
                context.Destroy();
                context = null;
            }
                
            clusterList.Dispose();
            vertexList.Dispose();
        }
    }

}
